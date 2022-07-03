using System.Text.Json;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.WebPubSub;
using Microsoft.Azure.WebPubSub.Common;
using Microsoft.Extensions.Logging;

namespace ChatFunc;

/// <summary>
///     Set the event handler upstream
///     URL Template: https://fca3-178-136-126-67.ngrok.io/runtime/webhooks/webpubsub
///     System events: connected, disconnected
///     User event: all 
/// </summary>
public static class WebPubSubFunctions
{
    [FunctionName("broadcast")]
    public static async Task Broadcast(
        [WebPubSubTrigger("ChatHub", WebPubSubEventType.User, "Message")] UserEventRequest request,
        BinaryData data,
        WebPubSubDataType dataType,
        [WebPubSub(Hub = "ChatHub")] IAsyncCollector<WebPubSubAction> actions,
        ILogger log)
    {
        log.LogInformation("User '{UserId}' has sent the message: {Body}", request.ConnectionContext.UserId, data);
    
        await actions.AddAsync(WebPubSubAction.CreateSendToAllAction(
            BinaryData.FromString($"{request.ConnectionContext.UserId}>{data}"),
            dataType));
    }

    [FunctionName("connected")]
    public static void Connected(
        [WebPubSubTrigger("ChatHub", WebPubSubEventType.System, "Connected")] ConnectedEventRequest request,
        [WebPubSub(Hub = "ChatHub")] IAsyncCollector<WebPubSubAction> action,
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
        [WebPubSubTrigger("ChatHub", WebPubSubEventType.System, "Disconnected")] DisconnectedEventRequest request,
        ILogger log)
    {
        log.LogInformation("User '{UseId}' disconnected", request.ConnectionContext.UserId);
    }
}
