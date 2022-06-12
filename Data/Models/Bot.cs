using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;
using WkHtmlWrapper.Image.Converters;

namespace weatherBot.Data.Models
{
    public class Bot
    {
        private TelegramBotClient _client;
        private readonly ApplicationDbContext _context;
        private ReplyKeyboardMarkup keyboard;

        private Dictionary<string, string> localizer = new Dictionary<string, string>
        {
            { "Clear", "Ясно"},
            {"Clouds", "Хмарно" },
            { "Thunderstorm", "Гроза"},
            { "Drizzle", "Мряка" },
            {"Rain", "Дощ" },
            {"Snow", "Сніг" },
            {"Mist", "Туман" },
            {"Smoke","Смок" },
            {"Haze", "Густий туман" },
            {"Dust","Курява" },
            {"Fog", "Туман " },
            {"Squall", "Шквал" },
            {"Tornado", "Торнадо" }
        };

        public Bot(ApplicationDbContext context)
        {
            _context = context;
            var temp = new List<KeyboardButton>();
            var names = _context.Regions.OrderByDescending(x=>x.Id).Select(x => x.Name).ToList();           
            CreateKeyboard(names.ToArray());
        }

        private void CreateKeyboard(string[] keyboardButtonNames)
        {
            var keyboard = new List<IEnumerable<KeyboardButton>>();
            foreach (var item in keyboardButtonNames)
            {
                keyboard.Add(new KeyboardButton[]
                {
                     new KeyboardButton(item)
                });
            }

            this.keyboard = new ReplyKeyboardMarkup(keyboard);
            this.keyboard.ResizeKeyboard = true;
        }



        public void Start()
        {
            _client = new TelegramBotClient(_context.Settings.FirstOrDefault(x => x.Id == -1).Token);
            using var cts = new CancellationTokenSource();

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { }
            };
            _client.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken: cts.Token);

            var me = _client.GetMeAsync().Result;
            Console.ReadLine();

            cts.Cancel();
        }

        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {

                if (update.Type != UpdateType.Message)
                    return;

                if (update.Message!.Type != MessageType.Text)
                    return;

                var chatId = update.Message.Chat.Id;
                var messageText = update.Message.Text;



                SendMessage(botClient, update.Message.Text, chatId);


            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message, e.StackTrace);
            }
        }

        private void SendMessage(ITelegramBotClient botClient, string command, long chatId)
        {
            try
            {
                if (_context.Regions.Select(x=>x.Name).Any(x => x.Contains(command)))
                {
                    var path = GetWeather(_context.Regions.FirstOrDefault(x => x.Name == command).Id);
                    if (path == null)
                        throw new Exception();
                    botClient.SendTextMessageAsync(chatId, "Ваша погода за запитом:", replyMarkup: keyboard);
                    SendPhotoMessage(chatId, "", path);
                    DeletePhoto(path);                    
                    return;
                }
                if (!string.IsNullOrEmpty(command) && command == "/start")
                {
                    botClient.SendTextMessageAsync(chatId, "Привіт!!!\nОберіть область для прогнозу погоди:", replyMarkup: keyboard);
                    return;
                }
                else
                {
                    botClient.SendTextMessageAsync(chatId, "Невірний ввід даних, будь ласка повторіть спробу", replyMarkup: keyboard);
                    return;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message, e.StackTrace);
            }
        }

        private void DeletePhoto(string path)
        {
            try
            {
                System.IO.File.Delete(path);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
            }
        }

        private void SendPhotoMessage(long chatId, string text, string photo)
        {
            try
            {
                using (var stream = System.IO.File.Open(photo, FileMode.Open))
                {
                    var res = _client.SendPhotoAsync(chatId, new InputOnlineFile(stream, photo), text);
                    res.Wait();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
            }
        }

        private string GetWeather(int regionId)
        {
            try
            {
                var weather = _context.Weathers.Include(x=>x.Region).Where(x=>x.RegionId == regionId).OrderBy(x=>x.Time).ToList();
                HtmlToImageConverter converter = new HtmlToImageConverter();
                string filePath = $"/temp/{DateTime.Now.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds.ToString()}.jpg";
                string html = "";

                using (var sr = new StreamReader(@"Weather.html"))
                {
                    html = sr.ReadToEnd();
                    sr.Dispose();
                }

                html = html.Replace("name", weather[0].Region.Name).Replace("weather", localizer[weather[0].WeatherMain])
                    .Replace("wind", weather[0].Wind_speed.ToString("0.00")).Replace("temp", weather[0].Temperature.ToString("0.00"));

                string result = Path.Combine(("/wwwroot" + filePath).Split('\\', '/'));
                var table = "<thead><tr>";
                for (int i = 1; i < weather.Count; i++)
                {
                    table += $"<td>{weather[i].Time}</td>";
                }
                table += "</tr></thead><tbody><tr>";

                for (int i = 1; i < weather.Count; i++)
                {
                    table += $"<th>{weather[i].Temperature.ToString("0.00")}°</th>";
                }
                table += "</tr><tr>";

                for (int i = 1; i < weather.Count; i++)
                {
                    table += $"<th>{localizer[weather[i].WeatherMain]}</th>";
                }
                table += "</tr></tbody>";
                html = html.Replace("mytable", table);

                converter.ConvertAsync(html, result).Wait();
                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
                return null;
            }
        }

        private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };


            return Task.CompletedTask;
        }
    }
}
