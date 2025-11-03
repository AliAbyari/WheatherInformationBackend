using RestSharp;
using WheatherInformation.Application.DTOs;

namespace WheatherInformation.Infrastructure.Remote.Interfaces
{
    public interface IOpenWeatherRemoteService
    {
        Task<RestResponse?> GetWeather(string city);
    }
}
