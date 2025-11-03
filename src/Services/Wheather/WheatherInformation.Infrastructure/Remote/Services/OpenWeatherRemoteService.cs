using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using SharedLibrary.Helpers;
using System.Net;
using WheatherInformation.Application.DTOs;
using WheatherInformation.Application.DTOs.Base;
using WheatherInformation.Infrastructure.Remote.Interfaces;

namespace WheatherInformation.Infrastructure.Remote.Base;

public class OpenWeatherRemoteService : IOpenWeatherRemoteService
{
    private readonly IOptions<OpenWeatherOptions> _opts;
    private readonly IOptions<SeriLogOptions> _serilog;

    public OpenWeatherRemoteService(
        IOptions<OpenWeatherOptions> opts,
        IOptions<SeriLogOptions> serilog)
    {
        _opts = opts;
        _serilog = serilog;
    }

    public async Task<RestResponse> GetWeather(string city)
    {
        if (string.IsNullOrWhiteSpace(city))
        {
            return new RestResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                ErrorMessage = "نام شهر نمی‌تواند خالی باشد."
            };
        }

        var apiKey = _opts.Value.ApiKey;
        var geoUrl = $"{_opts.Value.GeoUrl}{Uri.EscapeDataString(city)}&limit=1&appid={apiKey}";
        var weatherUrlBase = _opts.Value.WeatherUrl;
        var airUrlBase = _opts.Value.AirUrl;

        try
        {
            var geoClient = new RestClient(geoUrl);
            var geoRequest = new RestRequest();
            geoRequest.Method = Method.Get;
            var geoResponse = await geoClient.ExecuteAsync(geoRequest);

            if (!geoResponse.IsSuccessful)
                return new RestResponse
                {
                    StatusCode = HttpStatusCode.ServiceUnavailable,
                    ErrorMessage = "خطا در دریافت اطلاعات موقعیت جغرافیایی از سرویس OpenWeather"
                };

            var geos = JsonConvert.DeserializeObject<List<GeoResponse>>(geoResponse.Content ?? "[]");
            var geo = geos?.FirstOrDefault();
            if (geo == null)
                return new RestResponse
                {
                    StatusCode = HttpStatusCode.NotFound,
                    ErrorMessage = "شهر مورد نظر یافت نشد."
                };

            var weatherUrl = $"{weatherUrlBase}{geo.lat}&lon={geo.lon}&units=metric&appid={apiKey}";
            var weatherClient = new RestClient(weatherUrl);
            var weatherRequest = new RestRequest();
            weatherRequest.Method = Method.Get;
            var weatherResponse = await weatherClient.ExecuteAsync(weatherRequest);

            if (!weatherResponse.IsSuccessful)
                return new RestResponse
                {
                    StatusCode = HttpStatusCode.ServiceUnavailable,
                    ErrorMessage = "خطا در دریافت اطلاعات آب و هوایی"
                };

            var airUrl = $"{airUrlBase}{geo.lat}&lon={geo.lon}&appid={apiKey}";
            var airClient = new RestClient(airUrl);
            var airRequest = new RestRequest();
            airRequest.Method = Method.Get;
            var airResponse = await airClient.ExecuteAsync(airRequest);

            Helpers.InsertToLog(_serilog.Value.InsertInfoLog, "OpenWeather API Responses",
                $"Geo={geoResponse.Content} | Weather={weatherResponse.Content} | Air={airResponse.Content}");

            return new RestResponse
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonConvert.SerializeObject(new
                {
                    GeoContent = geoResponse.Content,
                    WeatherContent = weatherResponse.Content,
                    AirContent = airResponse.Content
                })
            };
        }
        catch (Exception ex)
        {
            Helpers.InsertToLog(_serilog.Value.InsertLog, "GetWeather Exception: " + ex);
            return new RestResponse
            {
                StatusCode = HttpStatusCode.InternalServerError,
                ErrorMessage = "خطای داخلی سرور",
                Content = ex.ToString()
            };
        }
    }
}
