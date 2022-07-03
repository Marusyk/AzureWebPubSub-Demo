using Azure.Messaging.WebPubSub;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp1.Controllers;

[ApiController]
[Route("[controller]")]
public class NegotiateController : ControllerBase
{
    private readonly WebPubSubServiceClient _webPubSubClient;
    private readonly ILogger<NegotiateController> _logger;

    public NegotiateController(WebPubSubServiceClient webPubSubClient, ILogger<NegotiateController> logger)
    {
        _webPubSubClient = webPubSubClient;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string id)
    {
        _logger.LogInformation("User {UserId} requested URL", id);

        var clientUrl = await _webPubSubClient.GetClientAccessUriAsync(userId: id);
        return new ObjectResult(new { url = clientUrl.AbsoluteUri });
    }
}