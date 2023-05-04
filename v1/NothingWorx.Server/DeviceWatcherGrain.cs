using Orleans.Runtime;

namespace NothingWorx.Server
{
    public interface IDeviceWatcherGrain : IGrainWithStringKey, IRemindable
    {
        Task WatchTelemetry(TelemetryMessage telemetryMessage);
    }

    public class DeviceWatcherGrain : Grain, IDeviceWatcherGrain
    {
        private const string ReminderToOnAlertUnsafeTemperatureDetected = nameof(ReminderToOnAlertUnsafeTemperatureDetected);
        private const string ReminderToOnAlertNoTelemetryDetected = nameof(ReminderToOnAlertNoTelemetryDetected);

        private const double UnsafeTemp = 120d;

        private readonly ILogger _logger;

        private readonly IPersistentState<DeviceState> _deviceState;

        public DeviceWatcherGrain(
            ILogger<DeviceWatcherGrain> logger,
            [PersistentState(stateName: "deviceState", storageName: "deviceStates")] IPersistentState<DeviceState> deviceState)
        {
            _logger = logger;

            _deviceState = deviceState;
        }

        public Task ReceiveReminder(string reminderName, TickStatus status)
        {
            if (reminderName == ReminderToOnAlertUnsafeTemperatureDetected)
            {
                _logger.LogError("Temp unsafe: deviceid = '{DeviceId}', temperature = '{Temperature}'", this.GetPrimaryKeyString(), _deviceState.State.CurrentTemperature);
            }
            else if (reminderName == ReminderToOnAlertNoTelemetryDetected)
            {
                _logger.LogWarning("No Telemetry: deviceid = '{DeviceId}'", this.GetPrimaryKeyString());
            }

            return Task.CompletedTask;
        }

        public async Task WatchTelemetry(TelemetryMessage telemetryMessage)
        {
            _deviceState.State = new DeviceState 
            { 
                CurrentTemperature = telemetryMessage.Temperature
            };

            await _deviceState.WriteStateAsync();

            var reminderToAlert = await this.GetReminder(ReminderToOnAlertUnsafeTemperatureDetected);

            if (telemetryMessage.Temperature > UnsafeTemp && reminderToAlert == null)
            {
                await this.RegisterOrUpdateReminder(ReminderToOnAlertUnsafeTemperatureDetected, TimeSpan.FromSeconds(1), TimeSpan.FromMinutes(1));
            }
            else if (telemetryMessage.Temperature < UnsafeTemp && reminderToAlert != null)
            {
                await this.UnregisterReminder(reminderToAlert);
            }

            await this.RegisterOrUpdateReminder(ReminderToOnAlertNoTelemetryDetected, TimeSpan.FromSeconds(1), TimeSpan.FromMinutes(1));

            _logger.LogInformation("message received: deviceid = '{DeviceId}', temperature = '{Temperature}'", this.GetPrimaryKeyString(), telemetryMessage.Temperature);
        }
    }

    [GenerateSerializer]
    public class TelemetryMessage
    {
        [Id(0)]
        public double Temperature { get; set; }
    }

    [GenerateSerializer]
    public class DeviceState
    {
        [Id(0)]
        public double CurrentTemperature { get; set; }
    }
}