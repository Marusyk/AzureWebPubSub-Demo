using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Azure.Messaging.WebPubSub;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace PubSubServer
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<MemoryStorage>();

            services.AddControllers();
            services.AddAzureClients(builder =>
            {
                var hubName = _configuration["Azure:WebPubSub:Hub"];
                builder.AddWebPubSubServiceClient(_configuration["Azure:WebPubSub:ConnectionString"], hubName);
            });
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "PubSubServer", Version = "v1"});
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "PubSubServer v1"));
            }

            // app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

                // https://{CONN}.ngrok.io/eventhandler  * connected,disconnected
                endpoints.Map("/eventhandler", async context =>
                {
                    var serviceClient = context.RequestServices.GetRequiredService<WebPubSubServiceClient>();
                    var memoryStorage = context.RequestServices.GetRequiredService<MemoryStorage>();

                    if (context.Request.Method == "OPTIONS")
                    {
                        if (context.Request.Headers["WebHook-Request-Origin"].Count > 0)
                        {
                            context.Response.Headers["WebHook-Allowed-Origin"] = "*";
                            context.Response.StatusCode = 200;
                            return;
                        }
                    }
                    else if (context.Request.Method == "POST")
                    {
                        // get the userId from header
                        var clientId = context.Request.Headers["ce-userId"];

                        if (context.Request.Headers["ce-type"] == "azure.webpubsub.sys.connected")
                        {
                            Console.WriteLine($"{clientId} connected");

                            var groupsAdded = memoryStorage.GetGroups(clientId)
                                .Select(group => serviceClient.AddUserToGroupAsync(group, clientId))
                                .ToList();

                            await Task.WhenAll(groupsAdded);
                            context.Response.StatusCode = 200;
                            return;
                        }

                        if (context.Request.Headers["ce-type"] == "azure.webpubsub.sys.disconnected")
                        {
                            Console.WriteLine($"{clientId} disconnected");

                            memoryStorage.Remove(clientId);
                            context.Response.StatusCode = 200;
                            return;
                        }

                        if (context.Request.Headers["ce-type"] == "azure.webpubsub.user.message")
                        {
                            using var stream = new StreamReader(context.Request.Body);
                            Console.WriteLine($"[{clientId}] {await stream.ReadToEndAsync()}");
                            context.Response.StatusCode = 200;
                            return;
                        }
                    }
                });
            });
        }
    }
}