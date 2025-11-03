namespace WheatherInformation.Application.DTOs.Base
{
    public class OpenWeatherOptions
    {
        public string ApiKey { get; set; }
        public string GeoUrl { get; set; }
        public string WeatherUrl { get; set; }
        public string AirUrl { get; set; }
    }

    public class SeriLogOptions
    {
        public bool InsertLog { get; set; }
        public bool InsertInfoLog { get; set; }
        public string LogFilePath { get; set; }
        public string LogFileName { get; set; }
        public int LogFileSizeMB { get; set; }

    }

    public class SecurityOptions
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
