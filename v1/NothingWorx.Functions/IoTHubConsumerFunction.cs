using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using IoTHubTrigger = Microsoft.Azure.WebJobs.EventHubTriggerAttribute;

namespace NoTwx.Functions
{
    public class IoTHubConsumerFunction
    {
        private static HttpClient client = new();

        [FunctionName("IoTHubConsumerFunction")]
        public static async Task Run([IoTHubTrigger("messages/events", Connection = "IoTHubConnection")]EventData message, ILogger log)
        {
            var deviceId = message.SystemProperties["iothub-connection-device-id"];
            var messageBody = JsonSerializer.Deserialize<MessageBody>(Encoding.UTF8.GetString(message.Body.Array));

            var response = await client.PostAsJsonAsync("https://localhost:7177/DeviceWatcherMessage?deviceId=" + deviceId, new PostBody
            {
                Temperature = messageBody.Temperature
            });

            response.EnsureSuccessStatusCode();

            log.LogInformation($"processed message from {deviceId}");
        }

        public class MessageBody
        {
            [JsonPropertyName("temperature")]
            public double Temperature { get; set; }
        }

        public class PostBody
        {
            [JsonPropertyName("temperature")]
            public double Temperature { get; set; }
        }
    }
}