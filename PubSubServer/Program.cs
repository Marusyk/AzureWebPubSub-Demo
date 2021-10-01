using System;
using Azure.Messaging.WebPubSub;
using Microsoft.VisualBasic;

var hubName = "stream";
var serviceClient = new WebPubSubServiceClient("", hubName);

Console.WriteLine("Enter message (format: groupName message):");

var streaming = Console.ReadLine();
while (streaming != null)
{
    var arr = streaming.Split(" ");
    // serviceClient.SendToGroup(arr[0], JsonSerializer.Serialize(new
    // {
    //     type = "sendToGroup",
    //     group = arr[0],
    //     dataType = "text",
    //     data = arr[1],
    //     ackId = ackId++
    // }));

    serviceClient.SendToGroup(arr[0].Trim(), arr[1].Trim());
    streaming = Console.ReadLine();
}

Console.WriteLine("Done");