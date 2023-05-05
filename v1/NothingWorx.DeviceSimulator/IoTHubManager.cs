using Microsoft.Azure.Devices;

namespace NothingWorx.DeviceSimulator
{
    public class IoTHubManager
    {
        readonly RegistryManager _registryManager;
        readonly string _iotHubHostname;

        public IoTHubManager(string iotHubConnectionString) 
        {
            _registryManager = RegistryManager.CreateFromConnectionString(iotHubConnectionString);
            _iotHubHostname = iotHubConnectionString.Split(';', '=')[1];   
        }

        public Task Open() => _registryManager.OpenAsync();

        public Task Close() => _registryManager.CloseAsync();

        public async Task<IoTHubDeviceManager> AddDevice(string deviceID) 
        {
            var device = await _registryManager.GetDeviceAsync(deviceID) ?? await _registryManager.AddDeviceAsync(new Device(deviceID));

            return new IoTHubDeviceManager(_iotHubHostname, device);
        }

        public async Task RemoveDevice(string deviceID)
        {
            var device = await _registryManager.GetDeviceAsync(deviceID);

            if (device != null)
            {
                await _registryManager.RemoveDeviceAsync(deviceID);
            }
        }

        public async Task ClearDevices()
        {
            var twins = await _registryManager.CreateQuery("select * from devices").GetNextAsTwinAsync();

            var devices = twins.Select(i => new Device(i.DeviceId));

            _ = await _registryManager.RemoveDevices2Async(devices, true, CancellationToken.None);
        }
    }
}
