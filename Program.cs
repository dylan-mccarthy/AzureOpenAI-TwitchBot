using System;
using System.Net.Sockets;
using System.Net.WebSockets;
using Microsoft.Extensions.Configuration;

public class Program
{
    public static void Main()
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        string? endpoint = config["OpenAIEndpoint"];
        Console.WriteLine($"Endpoint: {endpoint}");
        string? botName = config["BotName"];
        string? channelName = config["ChannelName"];
        string? redisConnectionString = config["RedisConnectionString"];

        RedisConnection redisConnection = new RedisConnection(redisConnectionString);

        Bot bot = new Bot(endpoint, botName, channelName, redisConnection);
        Console.ReadLine();
    }
}