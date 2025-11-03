namespace WheatherInformation.Application.DTOs;

public class AirPollutionResponse
{
    public List<AirItem>? list { get; set; }
}

public class AirItem
{
    public AirMain main { get; set; } = new();
    public Components components { get; set; } = new();
}

public class AirMain
{
    public int aqi { get; set; }
}

public class Components
{
    public double co { get; set; }
    public double no { get; set; }
    public double no2 { get; set; }
    public double o3 { get; set; }
    public double so2 { get; set; }
    public double pm2_5 { get; set; }
    public double pm10 { get; set; }
    public double nh3 { get; set; }

    public Dictionary<string, double> ToDictionary()
    {
        return new Dictionary<string, double>
        {
            ["co"] = co,
            ["no"] = no,
            ["no2"] = no2,
            ["o3"] = o3,
            ["so2"] = so2,
            ["pm2_5"] = pm2_5,
            ["pm10"] = pm10,
            ["nh3"] = nh3
        };
    }
}
