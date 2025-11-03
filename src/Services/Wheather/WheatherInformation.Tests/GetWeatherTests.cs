using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using RestSharp;
using System.Net;
using WheatherInformation.Api.Controllers;
using WheatherInformation.Application.DTOs;
using WheatherInformation.Application.DTOs.Base;
using WheatherInformation.Infrastructure.Remote.Base;
using WheatherInformation.Infrastructure.Remote.Interfaces;

namespace WheatherInformation.Tests;

public class GetWeatherTests
{
    [Fact]
    public async Task GetWeather_ReturnsBadRequest_WhenCityIsEmpty()
    {
        var mockWrapper = new Mock<IRemoteServiceWrapper>();
        var serilogOptions = Options.Create(new SeriLogOptions());
        var controller = new WeatherController(mockWrapper.Object, serilogOptions);

        var result = await controller.GetWeather("") as OkObjectResult;

        Assert.NotNull(result);

        var json = JsonConvert.SerializeObject(result.Value);
        var response = JsonConvert.DeserializeObject<ApiResponse>(json)!;

        Assert.Equal((int)HttpStatusCode.BadRequest, response.ResponseCode);
        Assert.Contains("نام شهر", response.Message);
    }

    [Fact]
    public async Task GetWeather_ReturnsOk_ForKnownCity()
    {
        var fakeWeather = new GetWeatherResponse
        {
            City = "Tehran",
            TemperatureC = 25.0,
            Humidity = 40,
            WindSpeed = 3.2,
            AirQualityIndex = 2,
            Latitude = 35.6892,
            Longitude = 51.3890
        };

        var fakeRestResponse = new RestResponse
        {
            StatusCode = HttpStatusCode.OK,
            Content = JsonConvert.SerializeObject(new
            {
                GeoContent = JsonConvert.SerializeObject(new[] { new { name = "Tehran", lat = 35.6892, lon = 51.3890 } }),
                WeatherContent = JsonConvert.SerializeObject(new { main = new { temp = 25.0, humidity = 40 }, wind = new { speed = 3.2 } }),
                AirContent = JsonConvert.SerializeObject(new { list = new[] { new { main = new { aqi = 2 } } } })
            })
        };

        var mockOpenWeather = new Mock<IOpenWeatherRemoteService>();
        mockOpenWeather.Setup(x => x.GetWeather("Tehran"))
            .ReturnsAsync(fakeRestResponse);

        var mockWrapper = new Mock<IRemoteServiceWrapper>();
        mockWrapper.Setup(x => x.OpenWeatherRemote).Returns(mockOpenWeather.Object);

        var serilogOptions = Options.Create(new SeriLogOptions());
        var controller = new WeatherController(mockWrapper.Object, serilogOptions);

        var result = await controller.GetWeather("Tehran") as OkObjectResult;

        Assert.NotNull(result);

        var json = JsonConvert.SerializeObject(result.Value);
        var response = JsonConvert.DeserializeObject<ApiResponse>(json)!;

        Assert.Equal((int)HttpStatusCode.OK, response.ResponseCode);
        Assert.Contains("درخواست", response.Message);
    }
}
