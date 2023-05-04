namespace NothingWorx.DeviceSimulator
{
    public partial class SimulatedDevice
    {
        readonly IoTHubDeviceManager _iotHubDeviceHelper;

        readonly System.Timers.Timer _timer = new System.Timers.Timer(5000)
        {
            AutoReset = true
        };

        public SimulatedDevice(IoTHubDeviceManager iotHubDeviceHelper, double temperature)
        {
            _iotHubDeviceHelper = iotHubDeviceHelper;

            _timer.Elapsed += (object? sender, System.Timers.ElapsedEventArgs e) =>
            {
                var messageBody = "{\"temperature\": " + temperature + "}";

                Task.Factory.StartNew(async () => { await _iotHubDeviceHelper.SendMessage(messageBody); });
            };
        }

        public string DeviceID => _iotHubDeviceHelper.DeviceID;

        public async Task Start()
        {
            await _iotHubDeviceHelper.Open();

            _timer.Start();
        }

        public async Task Stop()
        {
            _timer.Stop();

            await Task.Delay(5000);

            await _iotHubDeviceHelper.Close();
        }
    }
}
