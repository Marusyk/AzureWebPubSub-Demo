using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.WebPubSub;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace DeviceTelemetry.Server.Functions;

public class IoTHubFunctions
{
    [FunctionName("iot-message")]
    public async Task Run(
        [EventHubTrigger("iothub-ehub-wps-iot-20039802-79d870c1ea", Connection = "EventHubConnectionAppSetting")] string eventHubMessage,
        [WebPubSub(Hub = "TelemetryHub")] IAsyncCollector<WebPubSubAction> action,
        ILogger logger)
    {
        logger.LogInformation($"C# function triggered to process a message: {eventHubMessage}");

        var deviceMessage = BinaryData.FromString(eventHubMessage).ToObjectFromJson<DeviceMessage>(Default.JsonSerializerOptions);

        // send device data
        var temperatureMsg = new
        {
            messageId = deviceMessage.MessageId,
            deviceMessage.DeviceId,
            deviceMessage.Temperature
        };
        await Send("temperature", temperatureMsg);

        var humidityMsg = new
        {
            messageId = deviceMessage.MessageId,
            deviceMessage.DeviceId,
            deviceMessage.Humidity
        };
        await Send("humidity", humidityMsg);


        Task Send(string indicatorName, object message) =>
            action.AddAsync(WebPubSubAction.CreateSendToGroupAction($"{deviceMessage.DeviceId}_{indicatorName}".ToLowerInvariant(), JsonSerializer.Serialize(message, Default.JsonSerializerOptions)));
    }
}

public class DeviceMessage
{
    public int MessageId { get; set; }
    public string DeviceId { get; set; }
    public double Temperature { get; set; }
    public double Humidity { get; set; }
}