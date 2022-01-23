using Microsoft.Azure.WebPubSub.AspNetCore;
using Microsoft.Azure.WebPubSub.Common;

public class ChatHub : WebPubSubHub
{
    private readonly WebPubSubServiceClient<ChatHub> _serviceClient;
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(WebPubSubServiceClient<ChatHub> serviceClient, ILogger<ChatHub> logger)
    {
        _serviceClient = serviceClient;
        _logger = logger;
    }

    public override Task OnConnectedAsync(ConnectedEventRequest request)
    {
        string userId = request.ConnectionContext.UserId;

        _logger.LogInformation("User '{UseId}' connected", userId);

        return Task.CompletedTask;
    }

    public override Task OnDisconnectedAsync(DisconnectedEventRequest request)
    {
        string userId = request.ConnectionContext.UserId;

        _logger.LogInformation("User '{UseId}' disconnected", userId);

        return Task.CompletedTask;
    }

    public override async ValueTask<UserEventResponse> OnMessageReceivedAsync(UserEventRequest request, CancellationToken cancellationToken)
    {
        string userId = request.ConnectionContext.UserId;
        string body = request.Data.ToString();

        _logger.LogInformation("User '{UserId}' has sent the message: {Body}", userId, body);

        await _serviceClient.SendToAllAsync($"[{userId}] {body}");

        return request.CreateResponse(string.Empty);
    }
}
