using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Extensions;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;

public class Twitch
{
    private readonly TwitchClient client;

    public Twitch(string botName, string channelName)
    {
        ConnectionCredentials credentials = new ConnectionCredentials(botName, Environment.GetEnvironmentVariable("TwitchToken"));
        var clientOptions = new ClientOptions
        {
            MessagesAllowedInPeriod = 750,
            ThrottlingPeriod = TimeSpan.FromSeconds(30)
        };
        WebSocketClient customClient = new WebSocketClient(clientOptions);
        client = new TwitchClient(customClient);
        client.Initialize(credentials, channelName);
        Console.WriteLine($"Initialized Twitch client for {channelName}");
    }

    public void Connect()
    {
        client.Connect();
        Console.WriteLine($"Connected to Twitch");
    }

    public void Disconnect()
    {
        client.Disconnect();
    }

    public void SendMessage(string message)
    {
        client.SendMessage(client.JoinedChannels[0], message);
    }

    public void TimeoutUser(string channel, string username, TimeSpan time, string reason)
    {
        client.TimeoutUser(channel, username, time, reason);
    }
    
    public void SendWhisper(string username, string message)
    {
        client.SendWhisper(username, message);
    }

    public void AddOnLogHandler(EventHandler<OnLogArgs> handler)
    {
        client.OnLog += handler;
    }

    public void AddOnJoinedChannelHandler(EventHandler<OnJoinedChannelArgs> handler)
    {
        client.OnJoinedChannel += handler;
    }

    public void AddOnMessageReceivedHandler(EventHandler<OnMessageReceivedArgs> handler)
    {
        client.OnMessageReceived += handler;
    }

    public void AddOnWhisperReceivedHandler(EventHandler<OnWhisperReceivedArgs> handler)
    {
        client.OnWhisperReceived += handler;
    }

    public void AddOnNewSubscriberHandler(EventHandler<OnNewSubscriberArgs> handler)
    {
        client.OnNewSubscriber += handler;
    }

    public void AddOnConnectedHandler(EventHandler<OnConnectedArgs> handler)
    {
        client.OnConnected += handler;
    }
}