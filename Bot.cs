using System;
using TwitchLib.Client;
using TwitchLib.Client.Enums;
using TwitchLib.Client.Events;
using TwitchLib.Client.Extensions;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;

public class Bot
{
    TwitchClient client;
    AIChat aiChat;

    private string _endpoint;
    private string _botName;
    private string _channelName;

    public Bot(string? endpoint, string? botName, string? channelName)
    {
        _endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
        _botName = botName ?? throw new ArgumentNullException(nameof(botName));
        _channelName = channelName ?? throw new ArgumentNullException(nameof(channelName));

        aiChat = new AIChat(_endpoint);
        ConnectionCredentials credentials = new ConnectionCredentials(_botName, Environment.GetEnvironmentVariable("TwitchToken"));
        var clientOptions = new ClientOptions
        {
            MessagesAllowedInPeriod = 750,
            ThrottlingPeriod = TimeSpan.FromSeconds(30)
        };
        WebSocketClient customClient = new WebSocketClient(clientOptions);
        client = new TwitchClient(customClient);
        client.Initialize(credentials, _channelName);

        client.OnLog += Client_OnLog;
        client.OnJoinedChannel += Client_OnJoinedChannel;
        client.OnMessageReceived += Client_OnMessageReceived;
        client.OnWhisperReceived += Client_OnWhisperReceived;
        client.OnNewSubscriber += Client_OnNewSubscriber;
        client.OnConnected += Client_OnConnected;

        client.Connect();
    }

    private void Client_OnLog(object sender, OnLogArgs e)
    {
        Console.WriteLine($"{e.DateTime.ToString()}: {e.BotUsername} - {e.Data}");
    }

    private void Client_OnConnected(object sender, OnConnectedArgs e)
    {
        Console.WriteLine($"Connected to {e.AutoJoinChannel}");
    }

    private void Client_OnJoinedChannel(object sender, OnJoinedChannelArgs e)
    {
        Console.WriteLine("Hey guys! I am a bot connected via TwitchLib!");
        client.SendMessage(e.Channel, "Hey guys! I am a bot connected via TwitchLib!");
    }

    private void Client_OnMessageReceived(object sender, OnMessageReceivedArgs e)
    {
        if (e.ChatMessage.Message.Contains("badword"))
            client.TimeoutUser(e.ChatMessage.Channel, e.ChatMessage.Username, TimeSpan.FromMinutes(30), "Bad word! 30 minute timeout!");

        if (e.ChatMessage.Message.Contains($"@{_botName}"))
        {
            var returnMessage = aiChat.AskQuestion(e.ChatMessage.Message.Replace($"@{_botName}", ""));
            if(returnMessage.Count() >= 500)
            {
                client.SendMessage(e.ChatMessage.Channel, "Sorry, my response was too long let me summarize my answer");
                returnMessage = aiChat.SummarizeAnswer(returnMessage);
                client.SendMessage(e.ChatMessage.Channel, returnMessage);
            }
            else
                client.SendMessage(e.ChatMessage.Channel, returnMessage);
        }
    }
    
    private void Client_OnWhisperReceived(object sender, OnWhisperReceivedArgs e)
    {
        if (e.WhisperMessage.Username == "my_friend")
            client.SendWhisper(e.WhisperMessage.Username, "Hey! Whispers are so cool!!");
    }
    
    private void Client_OnNewSubscriber(object sender, OnNewSubscriberArgs e)
    {
        if (e.Subscriber.SubscriptionPlan == SubscriptionPlan.Prime)
            client.SendMessage(e.Channel, $"Welcome {e.Subscriber.DisplayName} to the substers! You just earned 500 points! So kind of you to use your Twitch Prime on this channel!");
        else
            client.SendMessage(e.Channel, $"Welcome {e.Subscriber.DisplayName} to the substers! You just earned 500 points!");
    }
}
