using System.Text.Json;
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

    public override async Task OnConnectedAsync(ConnectedEventRequest request)
    {
        string userId = request.ConnectionContext.UserId;

        _logger.LogInformation("User '{UseId}' connected", userId);
        
        var message = new
        {
            type = "system",
            @event = "message",
            data = $"{userId} connected"
        };
        await _serviceClient.SendToAllAsync($"Server>{JsonSerializer.Serialize(message)}");
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
        string message = request.Data.ToString();

        _logger.LogInformation("User '{UserId}' has sent the message: {Message}", userId, message);

        await _serviceClient.SendToAllAsync($"{userId}>{message}");

        return request.CreateResponse(string.Empty);
    }
}
