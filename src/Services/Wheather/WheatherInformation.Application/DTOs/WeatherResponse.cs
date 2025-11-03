namespace WheatherInformation.Application.DTOs;

public class WeatherResponse
{
    public WeatherMain main { get; set; } = new();
    public Wind? wind { get; set; }
}

public class WeatherMain
{
    public double temp { get; set; }
    public int pressure { get; set; }
    public int humidity { get; set; }
}

public class Wind
{
    public double speed { get; set; }
}

public class GetWeatherResponse
{
    public string City { get; set; } = string.Empty;
    public double TemperatureC { get; set; }
    public int Humidity { get; set; }
    public double WindSpeed { get; set; }
    public int AirQualityIndex { get; set; }
    public Dictionary<string, double> MajorPollutants { get; set; } = new();
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

