using WheatherInformation.Infrastructure.Remote.Interfaces;

namespace WheatherInformation.Infrastructure.Remote.Base
{
    public interface IRemoteServiceWrapper
    {
        IOpenWeatherRemoteService OpenWeatherRemote { get; }
    }
}
