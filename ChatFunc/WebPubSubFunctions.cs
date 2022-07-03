using System.Text.Json;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.WebPubSub;
using Microsoft.Azure.WebPubSub.Common;
using Microsoft.Extensions.Logging;

namespace ChatFunc;

/// <summary>
///     Set the event handler upstream
///     URL Template: <Function_App_Url>/runtime/webhooks/webpubsub?code=<API_KEY>
///     System events: connected, disconnected
///     User event: all 
/// </summary>
public static class WebPubSubFunctions
{
    [FunctionName("broadcast")]
    public static async Task Broadcast(
        [WebPubSubTrigger("ChatHubFunc", WebPubSubEventType.User, "Message")] UserEventRequest request,
        BinaryData data,
        WebPubSubDataType dataType,
        [WebPubSub(Hub = "ChatHubFunc")] IAsyncCollector<WebPubSubAction> actions,
        ILogger log)
    {
        log.LogInformation("User '{UserId}' has sent the message: {Body}", request.ConnectionContext.UserId, data);
    
        await actions.AddAsync(WebPubSubAction.CreateSendToAllAction(
            BinaryData.FromString($"{request.ConnectionContext.UserId}>{data}"),
            dataType));
    }

    [FunctionName("connected")]
    public static void Connected(
        [WebPubSubTrigger("ChatHubFunc", WebPubSubEventType.System, "Connected")] ConnectedEventRequest request,
        [WebPubSub(Hub = "ChatHubFunc")] IAsyncCollector<WebPubSubAction> action,
        ILogger log)
    {
        string userId = request.ConnectionContext.UserId;
        log.LogInformation("User '{UseId}' connected", userId);

        var message = new
        {
            type = "system",
            @event = "message",
            data = $"{userId} connected"
        };
        action.AddAsync(WebPubSubAction.CreateSendToAllAction(BinaryData.FromString($"Server>{JsonSerializer.Serialize(message)}"), WebPubSubDataType.Text));
    }

    [FunctionName("disconnect")]
    public static void Disconnected(
        [WebPubSubTrigger("ChatHubFunc", WebPubSubEventType.System, "Disconnected")] DisconnectedEventRequest request,
        [WebPubSub(Hub = "ChatHubFunc")] IAsyncCollector<WebPubSubAction> action,
        ILogger log)
    {
        string userId = request.ConnectionContext.UserId;
        log.LogInformation("User '{UseId}' disconnected", request.ConnectionContext.UserId);

        var message = new
        {
            type = "system",
            @event = "message",
            data = $"{userId} disconnected"
        };
        action.AddAsync(WebPubSubAction.CreateSendToAllAction(BinaryData.FromString($"Server>{JsonSerializer.Serialize(message)}"), WebPubSubDataType.Text));
    }
}
