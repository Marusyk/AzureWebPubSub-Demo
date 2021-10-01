using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using PubSubServer;

Host
    .CreateDefaultBuilder()
    .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); })
    .Build()
    .Run();