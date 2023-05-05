using System;
using Azure.AI.OpenAI;
using Azure.Identity;

public class AIChat
{
    private OpenAIClient _client;
    public AIChat(string endpoint){
        _client = new OpenAIClient(new Uri(endpoint), new DefaultAzureCredential());
    }

    public string AskQuestion(string question){
        var chatOptions = new ChatCompletionsOptions();
        chatOptions.Messages.Add(new ChatMessage(ChatRole.System,"You are a chat bot connected to Twitch.tv, please answer the following question nicely."));
        chatOptions.Messages.Add(new ChatMessage(ChatRole.User, question));
        var response = _client.GetChatCompletions("gpt3-5", chatOptions);
        return response.Value.Choices[0].Message.Content;
    }

    public string SummarizeAnswer(string answer){
        var chatOptions = new ChatCompletionsOptions();
        chatOptions.Messages.Add(new ChatMessage(ChatRole.System,"Please summarize the text provided to less then 500 characters."));
        chatOptions.Messages.Add(new ChatMessage(ChatRole.User, answer));
        var response = _client.GetChatCompletions("gpt3-5", chatOptions);
        return response.Value.Choices[0].Message.Content;
    }
}