using Spectre.Console;

namespace NothingWorx.DeviceSimulator
{
    public class SimulatedDeviceManager
    {
        readonly IoTHubManager _iotHubManager;
        readonly List<SimulatedDevice> _devices = new();

        public SimulatedDeviceManager(IoTHubManager ioTHubHelper) 
        {
            _iotHubManager = ioTHubHelper;
        }

        public async Task StartDevices(int deviceCount)
        {
            await AnsiConsole.Progress()
                .StartAsync(async ctx =>
                {
                    // Define tasks
                    var task1 = ctx.AddTask($"provisioning {deviceCount} devices");
                    var task2 = ctx.AddTask("starting devices");

                    var incrementValue = 100d / deviceCount;
                    
                    await _iotHubManager.Open();

                    var deviceIds = Array.CreateInstance(typeof(string), deviceCount);

                    for (var i = 0; i < deviceCount; i++)
                    {
                        var newDeviceID = "sim1device" + i;

                        var iotHubDeviceManager = await _iotHubManager.AddDevice(newDeviceID);

                        var newDevice = new SimulatedDevice(iotHubDeviceManager);

                        _devices.Add(newDevice);

                        task1.Increment(incrementValue);
                    }

                    var options = new ParallelOptions()
                    {
                        MaxDegreeOfParallelism = 10
                    };

                    await Parallel.ForEachAsync(_devices, options, async (i, ct) =>
                    {
                        await i.Start();

                        task2.Increment(incrementValue);
                    });
                });
        }

        public async Task StopDevices()
        {
            await AnsiConsole.Progress()
                .StartAsync(async ctx =>
                {
                    // Define tasks
                    var task1 = ctx.AddTask("stopping devices");

                    double incrementValue = 100d / _devices.Count;

                    var options = new ParallelOptions()
                    {
                        MaxDegreeOfParallelism = 10
                    };

                    await Parallel.ForEachAsync(_devices, options, async (i, ct) =>
                    {
                        await i.Stop();

                        task1.Increment(incrementValue);
                    });

                    _devices.Clear();
                });
        }
    }
}
