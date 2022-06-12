using NCrontab;
using System.Text.Json;
using weatherBot.Data;
using weatherBot.Data.Models;

namespace weatherBot.Services
{
    public class ParseWeatherService : BackgroundService
    {
        private CrontabSchedule _schedule;
        private DateTime _nextRun;
        private readonly HttpClient client;
        private readonly ApplicationDbContext _dbContext;
        private const string API_key = "2efe481e97a5b3cd8ba50954b056b3c7";
        private const string part = "hourly,minutely";

        private string Schedule => "* */20 * * * *";

        private Dictionary<int, string[]> points = new Dictionary<int, string[]>()
        {
            {-1, new string[] { "30.522093", "50.449777" } },
            {-2, new string[] { "28.471122", "49.230974" } },
            {-3, new string[] { "25.326611", "50.729812" } },
             {-4, new string[] { "35.049102", "48.455383" } },
            {-5, new string[] { "28.665727", "50.244946" } },
            {-6, new string[] { "22.288049", "48.618015" } },
             {-7, new string[] { "35.178773", "47.845394" } },
            {-8, new string[] { "24.714303", "48.906347" } },
            {-9, new string[] { "32.282877", "48.480117" } },
             {-10, new string[] { "39.309221", "48.562394" } },
            {-11, new string[] { "24.025683", "49.820111" } },
            {-12, new string[] { "32.004103", "46.993110" } },
             {-13, new string[] { "30.718307", "46.458617" } },
            {-14, new string[] { "34.558816", "49.556935" } },
            {-15, new string[] { "34.798076", "50.603149" } },
             {-16, new string[] { "34.798076", "50.902377" } },
            {-17, new string[] { "25.603589", "49.494448" } },
            {-18, new string[] { "32.624585", "46.579787" } },
             {-19, new string[] { "27.001168", "49.417337" } },
            {-20, new string[] { "32.050846", "49.421831" } },
            {-21, new string[] { "25.936365", "48.253408" } },
             {-22, new string[] { "31.343879", "51.472922" } }
        };

        public ParseWeatherService(IHost host)
        {
            this.client ??= new HttpClient();
            var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;
            _dbContext = services.GetRequiredService<ApplicationDbContext>();
            _schedule = CrontabSchedule.Parse(Schedule, new CrontabSchedule.ParseOptions { IncludingSeconds = true });
        }

        public void GetData()
        {
            try
            {
                var weathers = new List<Weather>();
                foreach (var point in points.Keys)
                {
                    var count = point;

                    for (int i = 0; i < points[point].Length; i += 2)
                    {
                        var result = client.GetAsync($"https://api.openweathermap.org/data/2.5/onecall?lat={points[point][i + 1]}&lon={points[point][i]}&exclude={part}&appid={API_key}");
                        result.Wait();
                        var data = JsonDocument.Parse(result.Result.Content.ReadAsStringAsync().Result).RootElement;

                        for (int j = 0; j < data.GetProperty("daily").GetArrayLength(); j++)
                        {
                            weathers.Add(new Weather
                            {
                                RegionId = point,
                                Temperature = data.GetProperty("daily")[j].GetProperty("temp").GetProperty("day").GetDouble() - 273.15,
                                Time = DateTime.Now.AddDays(j).Day,
                                Wind_speed = data.GetProperty("daily")[j].GetProperty("wind_speed").GetDouble(),
                                WeatherMain = data.GetProperty("daily")[j].GetProperty("weather")[0].GetProperty("main").GetString()
                            });
                        }

                    }
                }
                var temp = _dbContext.Weathers.ToList();

                if (temp.Count > 0)
                {
                    _dbContext.RemoveRange(temp);
                    _dbContext.SaveChanges();
                }
                foreach (var item in weathers)
                {
                    _dbContext.Add(item);
                }
                //_dbContext.AddRange(weathers);

                _dbContext.SaveChanges();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
            }
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            do
            {
                var now = DateTime.Now;
                if ((now.AddHours(3).Hour == 7 || now.AddHours(3).Hour == 10) || _dbContext.Weathers.ToList().Count == 0)
                {
                    GetData();
                    _nextRun = _schedule.GetNextOccurrence(DateTime.Now);
                }
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
            while (!stoppingToken.IsCancellationRequested);
        }
    }
}
