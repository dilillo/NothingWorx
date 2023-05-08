using Microsoft.Extensions.Configuration;
using Spectre.Console;
using Spectre.Console.Cli;

namespace NothingWorx.DeviceSimulator
{
    public class SimulateDevicesCommandSettings : CommandSettings
    {
        [CommandArgument(0, "[Count]")]
        public int Count { get; set; } = 10;
    }

    public class SimulateDevicesCommand : AsyncCommand<SimulateDevicesCommandSettings>
    {
        private readonly IoTHubManager _iotHubManager;

        public SimulateDevicesCommand(IConfiguration configuration)
        {
            var iotHubConnectionString = configuration.GetConnectionString("TargetIoTHub") ?? throw new ArgumentNullException("TargetIoTHub");

            _iotHubManager = new IoTHubManager(iotHubConnectionString);
        }

        public override async Task<int> ExecuteAsync(CommandContext context, SimulateDevicesCommandSettings settings)
        {
            try
            {
                await _iotHubManager.Open();

                var simulatedDeviceManager = new SimulatedDeviceManager(_iotHubManager);

                await simulatedDeviceManager.StartDevices(settings.Count);

                AnsiConsole.WriteLine("devices provisioned and transmitting.  press enter to stop ...");
                AnsiConsole.WriteLine();

                _ = Console.ReadLine();

                await simulatedDeviceManager.StopDevices();

                await _iotHubManager.Close();
            }
            catch (Exception ex)
            {
                AnsiConsole.WriteException(ex);
            }

            return 0;
        }
    }
}
