using Microsoft.AspNetCore.Mvc;
using System.Net;
using SharedLibrary.Helpers;
using WheatherInformation.Infrastructure.Remote.Base;
using Microsoft.Extensions.Options;
using WheatherInformation.Application.DTOs.Base;
using Serilog.Debugging;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using WheatherInformation.Application.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace WheatherInformation.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WeatherController : ControllerBase
    {
        private readonly IRemoteServiceWrapper _remote;
        private readonly IOptions<SeriLogOptions> _serilog;

        public WeatherController(IRemoteServiceWrapper remote, IOptions<SeriLogOptions> serilog)
        {
            _remote = remote;
            _serilog = serilog;

        }

        /// Basic YWRtaW46MTIzNA==
        [HttpGet("GetWeather")]
        [Authorize(AuthenticationSchemes = "BasicAuthentication")] 
        public async Task<IActionResult> GetWeather([FromQuery][Required] string city)
        {
            var now = DateTime.Now;

            try
            {
                if (string.IsNullOrWhiteSpace(city))
                {
                    return Ok(new
                    {
                        TimeStamp = now,
                        ResponseCode = HttpStatusCode.BadRequest,
                        Message = "نام شهر نمی‌تواند خالی باشد.",
                        Value = new { },
                        Error = new { }
                    });
                }

                var response = await _remote.OpenWeatherRemote.GetWeather(city);

                if (response == null || response.StatusCode != HttpStatusCode.OK)
                {
                    return Ok(new
                    {
                        TimeStamp = now,
                        ResponseCode = response?.StatusCode ?? HttpStatusCode.ServiceUnavailable,
                        Message = response?.ErrorMessage ?? "خطا در دریافت اطلاعات از سرویس رخ داده است.",
                        Value = new { },
                        Error = new { response?.ErrorMessage }
                    });
                }

                var allData = JsonConvert.DeserializeObject<OpenWeatherAllData>(response.Content ?? "{}");
                if (allData == null)
                {
                    return Ok(new
                    {
                        TimeStamp = now,
                        ResponseCode = HttpStatusCode.InternalServerError,
                        Message = "خطا در فراخوانی سرویس.",
                        Value = new { },
                        Error = new { }
                    });
                }

                var geoList = JsonConvert.DeserializeObject<List<GeoResponse>>(allData.GeoContent ?? "[]");
                var geo = geoList?.FirstOrDefault();

                var weather = JsonConvert.DeserializeObject<WeatherResponse>(allData.WeatherContent ?? "{}");
                var air = JsonConvert.DeserializeObject<AirPollutionResponse>(allData.AirContent ?? "{}");

                if (geo == null || weather == null)
                {
                    return Ok(new
                    {
                        TimeStamp = now,
                        ResponseCode = HttpStatusCode.NotFound,
                        Message = "شهر مورد نظر یافت نشد.",
                        Value = new { },
                        Error = new { }
                    });
                }

                var comp = air?.list?.FirstOrDefault()?.components;
                var pollutants = comp == null ? new Dictionary<string, double>() : new Dictionary<string, double>
                {
                    ["co"] = comp.co,
                    ["no"] = comp.no,
                    ["no2"] = comp.no2,
                    ["o3"] = comp.o3,
                    ["so2"] = comp.so2,
                    ["pm2_5"] = comp.pm2_5,
                    ["pm10"] = comp.pm10,
                    ["nh3"] = comp.nh3
                };

                var weatherResponse = new GetWeatherResponse
                {
                    City = geo.name ?? city,
                    TemperatureC = weather.main.temp,
                    Humidity = weather.main.humidity,
                    WindSpeed = weather.wind?.speed ?? 0,
                    Latitude = geo.lat,
                    Longitude = geo.lon,
                    AirQualityIndex = air?.list?.FirstOrDefault()?.main.aqi ?? 0,
                    MajorPollutants = pollutants
                };

                return Ok(new
                {
                    TimeStamp = now,
                    ResponseCode = HttpStatusCode.OK,
                    Message = "درخواست شما با موفقیت ارسال شد.",
                    Value = new { Response = weatherResponse },
                    Error = new { }
                });
            }
            catch (Exception ex)
            {
                Helpers.InsertToLog(_serilog.Value.InsertLog, "GetWeather - " + ex.ToString());

                return Ok(new
                {
                    TimeStamp = now,
                    ResponseCode = HttpStatusCode.InternalServerError,
                    Message = "خطا داخلی سرور رخ داده است.",
                    Value = new { },
                    Error = new { message = ex.ToString() }
                });
            }
        }


    }
}
