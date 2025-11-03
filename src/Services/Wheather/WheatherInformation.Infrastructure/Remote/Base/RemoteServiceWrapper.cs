using Microsoft.Extensions.Options;
using Serilog.Debugging;
using WheatherInformation.Application.DTOs.Base;
using WheatherInformation.Infrastructure.Remote.Interfaces;

namespace WheatherInformation.Infrastructure.Remote.Base
{
    public class RemoteServiceWrapper : IRemoteServiceWrapper
    {
        private readonly IOptions<OpenWeatherOptions> _opts;
        private readonly IOptions<SeriLogOptions> _serilog;

        public RemoteServiceWrapper(
            IOptions<OpenWeatherOptions> opts,
            IOptions<SeriLogOptions> serilog)
        {
            _opts = opts;
            _serilog = serilog;
        }

        public IOpenWeatherRemoteService OpenWeatherRemote =>
            new OpenWeatherRemoteService(_opts, _serilog);
    }
}
