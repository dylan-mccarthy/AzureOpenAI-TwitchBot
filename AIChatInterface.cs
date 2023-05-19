using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.SemanticFunctions;
using Microsoft.SemanticKernel.Planning;


using Azure.Identity;

public class AIChatInterface
{
    private readonly IKernel _kernel;
    public AIChatInterface(string endpoint)
    {
        var identity = new DefaultAzureCredential();

        _kernel = Kernel.Builder.Build();
        _kernel.Config.AddAzureChatCompletionService("gpt3-5",endpoint,identity);
        CreateChatRespondFunction();
        CreateResponseSummaryFunction();
        //CreateTellJokeFunction();
        
    }

    public async Task<string> Ask(string userQuery)
    {
        var planner = new SequentialPlanner(_kernel);
        
        var plan = await planner.CreatePlanAsync($"Please answer the following question"
        + "---Begin Question---"
        + $"{userQuery}"
        + @"---End Question---
        And then summarize your answer to less then 500 characters
        and verify the answer has addressed the question"
        );

        Console.WriteLine($"The plan has {plan.Steps.Count} steps");
        foreach(var step in plan.Steps){
            Console.WriteLine($"Step {step.Description}");
        }
        // if(_chatRespondFunction == null){
        //     throw new Exception("Chat Respond Function not created");
        // }
        // if(_responseSummaryFunction == null){
        //     throw new Exception("Response Summary Function not created");
        // }
        // var response = await _kernel.RunAsync(userQuery, _chatRespondFunction, _responseSummaryFunction);

        //return response.Result;

        var response = await _kernel.RunAsync(plan);
        return response.Result;
    }

    private void CreateVerificationFunction(){
        string veritifcationPrompt = @"Verify the answer test answers the question
        ---Begin Question---
        {{$QUESTION}}
        ---End Question---
        ---Begin Answer---
        {{$ANSWER}}
        ---End Answer---";

        var input = new PromptTemplateConfig.InputConfig();
        var inputParam = new PromptTemplateConfig.InputParameter();
        inputParam.Description = "The question to be answered";
        inputParam.Name = "QUESTION";

        var answerParam = new PromptTemplateConfig.InputParameter();
        answerParam.Description = "The answer to the question";
        answerParam.Name = "ANSWER";
        input.Parameters.Add(inputParam);
        input.Parameters.Add(answerParam);

        var promptConfig = new PromptTemplateConfig
        {
            Description = "Verifies the answer to the question",
            Completion = 
            {
                MaxTokens = 1000,
                Temperature = 0.2,
                TopP = 0.5
            },
            Input = input
        };

        var promptTemplate = new PromptTemplate(veritifcationPrompt, promptConfig,_kernel);
        var functionConfig = new SemanticFunctionConfig(promptConfig, promptTemplate);
        _kernel.RegisterSemanticFunction("TwitchSkill","VerifyAnswer", functionConfig);
    }

    private void CreateChatRespondFunction(){
        string ChatRespondPrompt = @"Answer the following question
         ---Begin Question---
            {{$QUESTION}}
          ---End Question---";

        var input = new PromptTemplateConfig.InputConfig();
        var inputParam = new PromptTemplateConfig.InputParameter();
        inputParam.Description = "The question to be answered";
        inputParam.Name = "QUESTION";

        var promptConfig = new PromptTemplateConfig
        {
            Description = "Answers questions from users",
            Completion = 
            {
                MaxTokens = 1000,
                Temperature = 0.2,
                TopP = 0.5
            },
            Input = input
        };

        var promptTemplate = new PromptTemplate(ChatRespondPrompt, promptConfig,_kernel);

        var functionConfig = new SemanticFunctionConfig(promptConfig, promptTemplate);

        _kernel.RegisterSemanticFunction("TwitchSkill","ChatRespond", functionConfig);

    }

    private void CreateResponseSummaryFunction(){
        string ChatRespondPrompt = @"Summarize the following to less then 500 characters
        ---Begin Text---
        {{$INPUT}}
        ---End Text---";

        var promptConfig = new PromptTemplateConfig
        {
            Description = "Summarize answer to less then 500 characters",
            Completion = 
            {
                MaxTokens = 1000,
                Temperature = 0.2,
                TopP = 0.5
            }
        };

        var promptTemplate = new PromptTemplate(ChatRespondPrompt, promptConfig,_kernel);

        var functionConfig = new SemanticFunctionConfig(promptConfig, promptTemplate);

        _kernel.RegisterSemanticFunction("TwitchSkill","SummarizeResponse", functionConfig);

    }
    private void CreateTellJokeFunction()
    {
        string tellJokePrompt = @"Tell a family friendly joke";

        var promptConfig = new PromptTemplateConfig
        {
            Description = "Tell a family friendly joke",
            Completion = 
            {
                MaxTokens = 1000,
                Temperature = 0.2,
                TopP = 0.5
            }
        };

        var promptTemplate = new PromptTemplate(tellJokePrompt, promptConfig,_kernel);

        var functionConfig = new SemanticFunctionConfig(promptConfig, promptTemplate);

        _kernel.RegisterSemanticFunction("TwitchSkill","TellJoke", functionConfig);

    }

}