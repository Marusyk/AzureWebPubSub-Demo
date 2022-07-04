using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.WebPubSub;
using Microsoft.Extensions.Logging;

namespace ChatFunc;

public static class NegotiateFunction
{
    [FunctionName("negotiate")]
    public static Response Negotiate(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req,
        [WebPubSubConnection(Hub = "ChatHubFunc", UserId = "{query.id}")] WebPubSubConnection connection,
        ILogger log)
    {
        string userId = req.Query["id"];
        log.LogInformation("User {UserId} requested URL", userId);

        return new Response(connection.Uri.AbsoluteUri);
    }
}

public record Response(string Url);
