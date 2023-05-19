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

    private readonly AIChat _aiChat;

    private string _endpoint;
    private string _botName;
    private string _channelName;
    private RedisConnection _redisConnection;

    private readonly Twitch _twitch;

    public Bot(string? endpoint, string? botName, string? channelName, RedisConnection redisConnection)
    {
        _endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
        _botName = botName ?? throw new ArgumentNullException(nameof(botName));
        _channelName = channelName ?? throw new ArgumentNullException(nameof(channelName));
        _redisConnection = redisConnection ?? throw new ArgumentNullException(nameof(redisConnection));

        _aiChat = new AIChat(_endpoint);
        _aiChat.MessageReceived += AiChat_MessageReceived;
        _twitch = new Twitch(_botName, _channelName);
        _twitch.AddOnLogHandler(Client_OnLog);
        _twitch.AddOnJoinedChannelHandler(Client_OnJoinedChannel);
        _twitch.AddOnMessageReceivedHandler(Client_OnMessageReceived);
        _twitch.AddOnWhisperReceivedHandler(Client_OnWhisperReceived);
        _twitch.AddOnNewSubscriberHandler(Client_OnNewSubscriber);
        _twitch.AddOnConnectedHandler(Client_OnConnected);
        _twitch.Connect();
        
    }

    private void AudioToText_TextReceived(object? sender, string e)
    {
        Console.WriteLine(e);
    }

    private void AiChat_MessageReceived(object? sender, string e)
    {
            if(e.Count() >= 500)
            {
                _twitch.SendMessage("Sorry, my response was too long let me summarize my answer");
                _twitch.SendMessage(_aiChat.SummarizeAnswer(e));
            }
            else
            {
                _twitch.SendMessage(e);
            }
    }

    private void Client_OnLog(object? sender, OnLogArgs e)
    {
        Console.WriteLine($"{e.DateTime.ToString()}: {e.BotUsername} - {e.Data}");
    }

    private void Client_OnConnected(object? sender, OnConnectedArgs e)
    {
        Console.WriteLine($"Connected to {e.AutoJoinChannel}");
    }

    private void Client_OnJoinedChannel(object? sender, OnJoinedChannelArgs e)
    {
        Console.WriteLine("Hey guys! I am a bot connected via TwitchLib!");
        _twitch.SendMessage("Hey guys! I am a bot connected via TwitchLib!");
    }

    private void Client_OnMessageReceived(object? sender, OnMessageReceivedArgs e)
    {
        if (e.ChatMessage.Message.Contains("badword"))
            _twitch.TimeoutUser(e.ChatMessage.Channel, e.ChatMessage.Username, TimeSpan.FromMinutes(30), "Bad word! 30 minute timeout!");

        if (e.ChatMessage.Message.Contains($"@{_botName}"))
        {
            _redisConnection.AddTwitchChatMessage(new TwitchChatMessage(e.ChatMessage.Username, e.ChatMessage.Message.Replace($"@{_botName}", "")));
            var messagesForUser = _redisConnection.GetTwitchChatMessageByUsername(e.ChatMessage.Username);
            _aiChat.AskQuestion(messagesForUser);
        }
    }
    
    private void Client_OnWhisperReceived(object? sender, OnWhisperReceivedArgs e)
    {
        if (e.WhisperMessage.Username == "my_friend")
            _twitch.SendWhisper(e.WhisperMessage.Username, "Hey! Whispers are so cool!!");
    }
    
    private void Client_OnNewSubscriber(object? sender, OnNewSubscriberArgs e)
    {
        if (e.Subscriber.SubscriptionPlan == SubscriptionPlan.Prime)
            _twitch.SendMessage($"Welcome {e.Subscriber.DisplayName} to the substers! You just earned 500 points! So kind of you to use your Twitch Prime on this channel!");
        else
            _twitch.SendMessage($"Welcome {e.Subscriber.DisplayName} to the substers! You just earned 500 points!");
    }
}
