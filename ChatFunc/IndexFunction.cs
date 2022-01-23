using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ChatFunc;

public static class IndexFunction
{
    [FunctionName("index")]
    public static IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous)] HttpRequest req, ILogger log)
    {
        string indexFile = "index.html";
        if (Environment.GetEnvironmentVariable("HOME") != null)
        {
            indexFile = Path.Join(Environment.GetEnvironmentVariable("HOME"), "site", "wwwroot", indexFile);
        }
        log.LogInformation($"index.html path: {indexFile}.");
        return new ContentResult
        {
            Content = File.ReadAllText(indexFile),
            ContentType = "text/html",
        };
    }
}
