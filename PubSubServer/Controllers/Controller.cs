using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Messaging.WebPubSub;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace PubSubServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class Controller : ControllerBase
    {
        private readonly ILogger<Controller> _logger;
        private readonly WebPubSubServiceClient _webPubSubServiceClient;
        private readonly MemoryStorage _memoryStorage;

        private readonly TimeSpan _tokenLifeTime = TimeSpan.FromDays(365);

        public Controller(ILogger<Controller> logger, WebPubSubServiceClient webPubSubServiceClient, MemoryStorage memoryStorage)
        {
            _logger = logger;
            _webPubSubServiceClient = webPubSubServiceClient;
            _memoryStorage = memoryStorage;
        }

        [HttpPost("subscriptions/{clientId}/equipments/{equipmentNumber}")]
        public async Task<Response> Subscribe(
            [FromRoute] string clientId,
            [FromRoute] string equipmentNumber,
            [FromBody] Request request)
        {
            _logger.LogInformation("Client '{ClientId}' subscribes for {EquipmentNumber} with fields {Fields}",
                clientId, equipmentNumber, string.Join(",", request.Fields));

            var subscription = new Subscription(equipmentNumber, request.Fields);
            _memoryStorage.Add(clientId, subscription);

            var url = await _webPubSubServiceClient.GenerateClientAccessUriAsync(
                roles: subscription.Groups.Select(group => $"webpubsub.joinLeaveGroup.{group}"),
                expiresAfter: _tokenLifeTime,
                userId: clientId);

            return new Response(url.AbsoluteUri);
        }

        [HttpPost("send")]
        public async Task Publish(Publish request)
        {
            _logger.LogInformation("Send value '{Value}' to group '{Group}'",
                request.Value, request.Group);

            await _webPubSubServiceClient.SendToGroupAsync(request.Group, request.Value);
        }
    }

    public record Request(IEnumerable<string> Fields);
    public record Response(string Url);
    public record Publish(string EquipmentNumber, string Field, string Value)
    {
        public string Group => $"{EquipmentNumber}_{Field}";
    }
}