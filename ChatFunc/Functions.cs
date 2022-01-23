using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.WebPubSub;
using Microsoft.Azure.WebPubSub.Common;
using Microsoft.Extensions.Logging;

namespace ChatFunc;

public static class Functions
{
    [FunctionName("broadcast")]
    public static async Task<UserEventResponse> Broadcast(
        [WebPubSubTrigger("chat", WebPubSubEventType.User, "message")] UserEventRequest request,
        BinaryData data,
        WebPubSubDataType dataType,
        [WebPubSub(Hub = "chat")] IAsyncCollector<WebPubSubAction> actions,
        ILogger log)
    {
        log.LogInformation("User '{UserId}' has sent the message: {Body}", request.ConnectionContext.UserId, data);

        await actions.AddAsync(WebPubSubAction.CreateSendToAllAction(
            BinaryData.FromString($"[{request.ConnectionContext.UserId}] {data}"),
            dataType));

        return new UserEventResponse
        {
            Data = BinaryData.FromString(string.Empty),
            DataType = WebPubSubDataType.Text
        };
    }

    [FunctionName("connected")]
    public static void Connected(
        [WebPubSubTrigger("chat", WebPubSubEventType.System, "connected")] WebPubSubConnectionContext connectionContext,
        ILogger log)
    {
        log.LogInformation("User '{UseId}' connected", connectionContext.UserId);
    }

    [FunctionName("disconnect")]
    public static void Disconnected(
        [WebPubSubTrigger("chat", WebPubSubEventType.System, "disconnected")] WebPubSubConnectionContext connectionContext,
        ILogger log)
    {
        log.LogInformation("User '{UseId}' disconnected", connectionContext.UserId);
    }
}
