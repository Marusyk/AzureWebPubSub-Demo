using DeviceTelemetryServer;
using Microsoft.Azure.WebPubSub.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddWebPubSub(o => o.ServiceEndpoint = new ServiceEndpoint(builder.Configuration["Azure:WebPubSub:ConnectionString"]))
    .AddWebPubSubServiceClient<TelemetryHub>();

builder.Services.AddSingleton<MemoryStorage>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapWebPubSubHub<TelemetryHub>("/events/{*path}");

app.Run();