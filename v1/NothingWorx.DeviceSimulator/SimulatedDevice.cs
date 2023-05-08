using Spectre.Console;

namespace NothingWorx.DeviceSimulator
{
    public partial class SimulatedDevice
    {
        readonly IoTHubDeviceManager _iotHubDeviceManager;
        readonly Random _random = new();

        readonly System.Timers.Timer _timer = new(5000)
        {
            AutoReset = true
        };

        public SimulatedDevice(IoTHubDeviceManager iotHubDeviceHelper)
        {
            _iotHubDeviceManager = iotHubDeviceHelper;

            _timer.Elapsed += (object? sender, System.Timers.ElapsedEventArgs e) =>
            {
                _ = Task.Factory.StartNew(async () =>
                {
                    var messageBody = "{\"temperature\": " + Math.Round(_random.NextDouble() * 100d, 1) + "}";

                    await _iotHubDeviceManager.SendMessage(messageBody);

                    AnsiConsole.WriteLine($"{DeviceID} sent {messageBody}");
                });
            };
        }

        public string DeviceID => _iotHubDeviceManager.DeviceID;

        public async Task Start()
        {
            await _iotHubDeviceManager.Open();

            _timer.Start();
        }

        public async Task Stop()
        {
            _timer.Stop();

            await Task.Delay(5000);

            await _iotHubDeviceManager.Close();
        }
    }
}
