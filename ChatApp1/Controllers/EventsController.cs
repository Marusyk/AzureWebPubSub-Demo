using Azure.Messaging.WebPubSub;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.Controllers;

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

    // abuse protection of cloudevents
    [HttpOptions]
    public IActionResult Options()
    {
        if (Request.Headers["WebHook-Request-Origin"].Count > 0)
        {
            Response.Headers.Add("WebHook-Allowed-Origin", "*");
            return Ok();
        }
    
        return BadRequest();
    }

    [HttpPost]
    public async Task<IActionResult> Handle()
    {
        var eventType = Request.Headers["ce-type"].ToString();
        var userId = Request.Headers["ce-userId"].ToString();

        if (eventType == "azure.webpubsub.sys.connected")
        {
            _logger.LogInformation("User '{UseId}' connected", userId);
        }
        else if (eventType == "azure.webpubsub.sys.disconnected")
        {
            _logger.LogInformation("User '{UseId}' disconnected", userId);
        }
        else if (eventType == "azure.webpubsub.user.message")
        {
            using var stream = new StreamReader(Request.Body);
            var body = await stream.ReadToEndAsync();

            _logger.LogInformation("User '{UserId}' has sent the message: {Body}", userId, body);

            await _webPubSubClient.SendToAllAsync($"[{userId}] {body}");
        }

        return Ok();
    }
}