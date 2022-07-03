using System.Text.Json;
using Azure.Messaging.WebPubSub;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp1.Controllers;

[ApiController]
[Route("[controller]")]
public class EventsController : ControllerBase
{
    private readonly WebPubSubServiceClient _webPubSubClient;
    private readonly ILogger<EventsController> _logger;

    public EventsController(WebPubSubServiceClient webPubSubClient, ILogger<EventsController> logger)
    {
        _webPubSubClient = webPubSubClient;
        _logger = logger;
    }

    [HttpOptions]
    public IActionResult Options() // abuse protection of cloudevents
    {
        if (Request.Headers["WebHook-Request-Origin"].Count <= 0)
        {
            return BadRequest();
        }

        Response.Headers.Add("WebHook-Allowed-Origin", "*");
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> Handle()
    {
        // https://docs.microsoft.com/en-us/azure/azure-web-pubsub/reference-cloud-events
        var eventType = Request.Headers["ce-type"].ToString(); 
        var userId = Request.Headers["ce-userId"].ToString();

        if (eventType == "azure.webpubsub.sys.connected")
        {
            _logger.LogInformation("User '{UseId}' connected", userId);

            var message = new
            {
                type = "system",
                @event = "message",
                data = $"{userId} connected"
            };
            await _webPubSubClient.SendToAllAsync($"Server>{JsonSerializer.Serialize(message)}");
        }
        else if (eventType == "azure.webpubsub.sys.disconnected")
        {
            _logger.LogInformation("User '{UseId}' disconnected", userId);

            var message = new
            {
                type = "system",
                @event = "message",
                data = $"{userId} disconnected"
            };
            await _webPubSubClient.SendToAllAsync($"Server>{JsonSerializer.Serialize(message)}");
        }
        else if (eventType == "azure.webpubsub.user.message")
        {
            using var stream = new StreamReader(Request.Body);
            var message = await stream.ReadToEndAsync();

            _logger.LogInformation("User '{UserId}' has sent the message: {Message}", userId, message);

            await _webPubSubClient.SendToAllAsync($"{userId}>{message}");
        }

        return Ok();
    }
}