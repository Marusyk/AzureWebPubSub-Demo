using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Messaging.WebPubSub;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace RealTimeServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NegotiateController : ControllerBase
    {
        private readonly ILogger<NegotiateController> _logger;
        private readonly WebPubSubServiceClient _webPubSubServiceClient;

        public NegotiateController(ILogger<NegotiateController> logger, WebPubSubServiceClient webPubSubServiceClient)
        {
            _logger = logger;
            _webPubSubServiceClient = webPubSubServiceClient;
        }

        [HttpPost("{clientId}/equipments/{equipmentNumber}")]
        public async Task<Response> Get(
            [FromRoute] string clientId,
            [FromRoute] string equipmentNumber,
            [FromBody] Request request)
        {
            _logger.LogInformation($"Negotiate with client '{clientId}'. Subscribe for {equipmentNumber} with fields {string.Join(",", request.Fields)}");

            var groups = request.Fields
                .Select(field => $"{equipmentNumber}_{field}")
                .ToList();

            var url = await _webPubSubServiceClient.GenerateClientAccessUriAsync(
                roles: groups.Select(group => $"webpubsub.joinLeaveGroup.{group}"),
                expiresAfter: TimeSpan.FromDays(365),
                userId: clientId);

            return new Response(url.AbsoluteUri, groups);
        }
    }

    public record Request(IEnumerable<string> Fields);
    public record Response(string Url, IEnumerable<string> Groups);
}