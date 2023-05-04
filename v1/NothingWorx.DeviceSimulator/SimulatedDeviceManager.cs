namespace NothingWorx.DeviceSimulator
{
    public class SimulatedDeviceManager
    {
        public SimulatedDeviceManager(IoTHubManager ioTHubHelper) 
        {
            _ioTHubHelper = ioTHubHelper;
        }

        readonly IoTHubManager _ioTHubHelper;
        
        List<SimulatedDevice> _devices = new();

        public async Task StartDevices(int deviceCount)
        {
            var random = new Random();

            await _ioTHubHelper.Open();
            
            for (var i = 0; i < deviceCount; i++)
            {
                var newDeviceID = RandomDeviceID;

                var iotHubDeviceHelper = await _ioTHubHelper.AddDevice(newDeviceID);

                var newDevice = new SimulatedDevice(iotHubDeviceHelper, random.NextDouble() * 100d);

                await newDevice.Start();

                _devices.Add(newDevice);
            }
        }

        public async Task StopDevices()
        {
            await Task.Run(() => Parallel.ForEach(_devices, async (i) => await i.Stop()));

            _devices.Clear();
        }

        static string RandomDeviceID => "device" + new string(Guid.NewGuid().ToString().TakeLast(4).ToArray());
    }
}
