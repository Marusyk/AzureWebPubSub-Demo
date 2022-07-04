using System.Net.WebSockets;
using Websocket.Client;
using static DeviceTelemetry.ConsoleClient.Helpers;

var deviceId = ReadDeviceId("1032888");

Console.WriteLine("Enter indicators by comma (temperature,humidity):");
var indicators = Console.ReadLine()?.Split(',');

// Negotiate
var response = await Negotiate("https://wps-iot.azurewebsites.net", deviceId, indicators);

// Connect
using var client = new WebsocketClient(new Uri(response.Url), () =>
{
    var inner = new ClientWebSocket();
    return inner;
});
client.ReconnectTimeout = null;
client.MessageReceived.Subscribe(msg =>
{
    Print(msg.Text);
});

await client.Start();
Console.WriteLine("Connected");

Console.Read();