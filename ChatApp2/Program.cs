using Microsoft.Azure.WebPubSub.AspNetCore;

var builder = WebApplication.CreateBuilder();

builder.Services
    .AddWebPubSub(o => o.ServiceEndpoint = new ServiceEndpoint(builder.Configuration["Azure:WebPubSub:ConnectionString"]))
    .AddWebPubSubServiceClient<ChatHub>();

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
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapWebPubSubHub<ChatHub>("/events/{*path}");

app.MapGet("/negotiate", async context =>
{
    var id = context.Request.Query["id"];
    if (id.Count != 1)
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync("missing user id");
        return;
    }
    var serviceClient = context.RequestServices.GetRequiredService<WebPubSubServiceClient<ChatHub>>();
    await context.Response.WriteAsync(serviceClient.GetClientAccessUri(userId: id).AbsoluteUri);
});

app.Run();