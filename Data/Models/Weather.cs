namespace weatherBot.Data.Models
{
    public class Weather
    {
        public int Id { get; set; }
        public double Temperature { get; set; }
        public int Time { get; set; }
        public string WeatherMain { get; set; }
        public double Wind_speed { get; set; }
        public int RegionId { get; set; }
        public Region Region { get; set; }
    }
}
