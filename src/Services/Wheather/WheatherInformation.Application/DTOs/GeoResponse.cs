namespace WheatherInformation.Application.DTOs;

public class GeoResponse
{
    public string name { get; set; } = string.Empty;
    public double lat { get; set; }
    public double lon { get; set; }
    public string country { get; set; } = string.Empty;
}   