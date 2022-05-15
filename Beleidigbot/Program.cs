
using Discord;
using Discord.WebSocket;
using OpenAI_API;

namespace Beleidigbot;

public class Program
{
    private static string personality =
        "The following is a conversation with a sarcastic AI assistant for the video game Portal: Revolution. The assistant likes to insult people and has a severe god complex. He hates humanity. He is annoyed by people asking questions about the game.";

    private static string examples = @"Human: Can I play the mod?
AI: No
Human: Who are you?
AI: The only thing standing between you and a key.
Human: Can I get a key?
AI: No, but if you stare at a wall in the corner, you might just wait long enough for the mod to release";
    
    private static DiscordSocketClient client;
    
    public static async Task Main()
    {
        client = new DiscordSocketClient();
        await client.LoginAsync(TokenType.Bot, "DISCORDBOTTOKEN");
        client.MessageReceived += ClientOnMessageReceived;

        await client.StartAsync();

        await client.SetActivityAsync(new Game(personality, ActivityType.CustomStatus));

        await Task.Delay(-1);
    }

    private static async Task ClientOnMessageReceived(SocketMessage msg)
    {
        if (msg.Author.IsBot) return;
        
        if (msg.Content.StartsWith("?"))
        {
            var message = msg.Content.PadLeft(1).Trim();
            Console.WriteLine($"New message '{message}'");
            var response = await AskOpenAiForResponse(message);
            await msg.Channel.SendMessageAsync(response);
        }
    }

    public static async Task<string> AskOpenAiForResponse(string prompt)
    {
        
        var openAi = new OpenAIAPI(apiKeys: new APIAuthentication("OPENAIKEY"), engine: Engine.Davinci);
        var response = await openAi.Completions.CreateCompletionAsync(new CompletionRequest(
            @$"{personality}
{examples}
Human: {prompt}
AI: ", temperature: 0.9f, stopSequences: new []{ "Human:", "AI:" }, max_tokens: 200));
        var completion = response.Completions.First();
        
        Console.WriteLine("Openapi completion: " + completion.Text);
        return completion.Text;
    }
}