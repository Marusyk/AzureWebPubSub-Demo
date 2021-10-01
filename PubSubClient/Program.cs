using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading.Tasks;
using Websocket.Client;

Console.WriteLine("Enter equipment number:");
var equipmentNumber = Console.ReadLine();

Console.WriteLine("Enter fields by comma:");
var fields = Console.ReadLine()?.Split(',');

// Negotiate
var response = await Negotiate("https://localhost:5001", equipmentNumber, fields);

// Connect
using var client = new WebsocketClient(new Uri(response.Url), () =>
{
    var inner = new ClientWebSocket();
    inner.Options.AddSubProtocol("json.webpubsub.azure.v1");
    return inner;
});
client.ReconnectTimeout = null;
client.MessageReceived.Subscribe(msg =>
{
    Console.BackgroundColor = ConsoleColor.Green;
    Console.WriteLine($"Message received: {msg}");
    Console.ResetColor();
});
await client.Start();
Console.WriteLine("Connected");

// Joining groups
var ackId = 1;
foreach (var group in response.Groups)
{
    Console.WriteLine($"Joining group: {group}");
    client.Send(JsonSerializer.Serialize(new
    {
        type = "joinGroup",
        group = group,
        ackId = ackId++
    }));
}

Console.Read();

async Task<Response> Negotiate(string host, string equipmentNumber, IEnumerable<string> fields)
{
    const string clientId = "Client1";
    
    using HttpClient httpClient = new();
    var responseMessage = await httpClient.PostAsJsonAsync(
        $"{host}/negotiate/{clientId}/equipments/{equipmentNumber}", new
        {
            Fields = fields
        });

    responseMessage.EnsureSuccessStatusCode();

    return await responseMessage.Content.ReadFromJsonAsync<Response>(new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    });
}

public record Response(string Url, IEnumerable<string> Groups);