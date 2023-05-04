using Microsoft.AspNetCore.Mvc;

namespace NothingWorx.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DeviceWatcherMessageController : ControllerBase
    {
        private readonly ILogger<DeviceWatcherMessageController> _logger;

        private readonly IGrainFactory _grains;

        public DeviceWatcherMessageController(ILogger<DeviceWatcherMessageController> logger, IGrainFactory grains)
        {
            _logger = logger;

            _grains = grains;
        }

        [HttpPost]
        public async Task<IResult> Post([FromQuery]string deviceId, [FromBody]TelemetryMessage model)
        {
            var grain = _grains.GetGrain<IDeviceWatcherGrain>(deviceId);

            await grain.WatchTelemetry(model);

            return Results.Ok();
        }
    }
}