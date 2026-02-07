using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using LuauGotoStripper;
using Microsoft.Extensions.DependencyInjection;

var token = Config.Token;
if (string.IsNullOrWhiteSpace(token) || token == "yourbottokenhere")
{
    Console.WriteLine("Token is missing (set Config.Token).");
    return;
}

var client = new DiscordSocketClient(new DiscordSocketConfig
{
    GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.MessageContent
});

var interactionService = new InteractionService(client.Rest);
var commandService = new CommandService(new CommandServiceConfig
{
    CaseSensitiveCommands = false
});

var services = new ServiceCollection()
    .AddSingleton(client)
    .AddSingleton(interactionService)
    .AddSingleton(commandService)
    .AddSingleton(new HttpClient())
    .AddSingleton<CommandHandler>()
    .BuildServiceProvider();

client.Log += LogAsync;
interactionService.Log += LogAsync;
commandService.Log += LogAsync;

await services.GetRequiredService<CommandHandler>().InitializeAsync();

await client.LoginAsync(TokenType.Bot, token);
await client.StartAsync();

await Task.Delay(Timeout.Infinite);

static Task LogAsync(LogMessage message)
{
    Console.WriteLine($"[{DateTimeOffset.Now:O}] {message.Severity}: {message.Source} - {message.Message}");
    if (message.Exception != null)
    {
        Console.WriteLine(message.Exception);
    }
    return Task.CompletedTask;
}

