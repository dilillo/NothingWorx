using Microsoft.Extensions.Configuration;
using NothingWorx.DeviceSimulator;

try
{
    var configBuilder = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddEnvironmentVariables()
        .AddJsonFile("appsettings.json")
        .AddJsonFile("appsettings.Development.json", true);

    var rootConfig = configBuilder.Build();

    var iotHubManager = new IoTHubManager(rootConfig.GetConnectionString("TargetIoTHub"));

    var simulatedDeviceManager = new SimulatedDeviceManager(iotHubManager);

    await iotHubManager.Open();

    await simulatedDeviceManager.StartDevices(int.Parse(rootConfig["DeviceCount"]));

    Console.WriteLine("devices started.  press enter to quit ...");

    _ = Console.ReadLine();

    await simulatedDeviceManager.StopDevices();

    await iotHubManager.ClearDevices();

    await iotHubManager.Close();
}
catch (Exception ex)
{
    Console.WriteLine(ex);
}
