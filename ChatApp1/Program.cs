using Microsoft.Extensions.Azure;

var builder = WebApplication.CreateBuilder();

builder.Services.AddAzureClients(clientBuilder =>
{
    clientBuilder.AddWebPubSubServiceClient(
        connectionString: builder.Configuration["Azure:WebPubSub:ConnectionString"],
        hub: builder.Configuration["Azure:WebPubSub:Hub"]);
});

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
app.MapControllers();
app.Run();