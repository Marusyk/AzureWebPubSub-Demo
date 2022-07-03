using System.Net.Http.Json;
using System.Net.Mime;
using System.Text;
using System.Text.Json;

namespace DeviceTelemetryClient;

public static class Helpers
{
    public static async Task<Response> Negotiate(string host, string deviceId, IEnumerable<string> indicators)
    {
        const string clientId = "ConsoleClient1";

        var requestBody = new
        {
            indicators
        };
        using var httpMessage = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, MediaTypeNames.Application.Json),
            RequestUri = new Uri($"{host}/api/subscriptions/{clientId}/devices/{deviceId}")
        };

        using var httpClient = new HttpClient();
        using var responseMessage = await httpClient.SendAsync(httpMessage);

        responseMessage.EnsureSuccessStatusCode();

        return await responseMessage.Content.ReadFromJsonAsync<Response>(new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    public static string ReadDeviceId(string deviceId)
    {
        Console.Write($"The device ID is '{deviceId}'. Please confirm (Y/n): ");
        var confirmation = Console.ReadLine();
        switch (confirmation)
        {
            case "y":
            case "Y":
                return deviceId;
            case "n":
            case "N":
                Console.WriteLine("Enter the desired device ID:");
                return Console.ReadLine();
            default:
                return ReadDeviceId(deviceId);
        }
    }

    public static void Print(string message)
    {
        Console.BackgroundColor = message.Contains("humidity") ? ConsoleColor.DarkBlue : ConsoleColor.Green;
        Console.WriteLine($"Message received: {message}");
        Console.ResetColor();
    }
}

public record Response(string Url);