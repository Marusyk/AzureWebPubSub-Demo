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

        public Controller(ILogger<Controller> logger, WebPubSubServiceClient webPubSubServiceClient)
        {
            _logger = logger;
            _webPubSubServiceClient = webPubSubServiceClient;
        }

        [HttpPost("negotiate/{clientId}/equipments/{equipmentNumber}")]
        public async Task<Response> Negotiate(
            [FromRoute] string clientId,
            [FromRoute] string equipmentNumber,
            [FromBody] Request request)
        {
            _logger.LogInformation("Client '{ClientId}' subscribes for {EquipmentNumber} with fields {Fields}",
                clientId, equipmentNumber, string.Join(",", request.Fields));

            var groups = request.Fields
                .Select(field => $"{equipmentNumber}_{field}")
                .ToList();

            var url = await _webPubSubServiceClient.GenerateClientAccessUriAsync(
                roles: groups.Select(group => $"webpubsub.joinLeaveGroup.{group}"),
                expiresAfter: TimeSpan.FromDays(365),
                userId: clientId);

            return new Response(url.AbsoluteUri, groups);
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
    public record Response(string Url, IEnumerable<string> Groups);
    public record Publish(string EquipmentNumber, string Field, string Value)
    {
        public string Group => $"{EquipmentNumber}_{Field}";
    }
}