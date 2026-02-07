using System.Reflection;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;

namespace LuauGotoStripper;

public sealed class CommandHandler
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _interactionService;
    private readonly CommandService _commandService;
    private readonly IServiceProvider _services;

    public CommandHandler(
        DiscordSocketClient client,
        InteractionService interactionService,
        CommandService commandService,
        IServiceProvider services)
    {
        _client = client;
        _interactionService = interactionService;
        _commandService = commandService;
        _services = services;
    }

    public async Task InitializeAsync()
    {
        await _interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        await _commandService.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

        _client.Ready += OnReadyAsync;
        _client.InteractionCreated += OnInteractionCreatedAsync;
        _client.MessageReceived += OnMessageReceivedAsync;
    }

    private async Task OnReadyAsync()
    {
        var guild = Config.Guild;
        if (string.IsNullOrWhiteSpace(guild))
        {
            await _interactionService.RegisterCommandsGloballyAsync(true);
            return;
        }

        if (ulong.TryParse(guild, out var guildId))
        {
            await _interactionService.RegisterCommandsToGuildAsync(guildId, true);
        }
        else
        {
            await _interactionService.RegisterCommandsGloballyAsync(true);
        }
    }

    private async Task OnInteractionCreatedAsync(SocketInteraction interaction)
    {
        var ctx = new SocketInteractionContext(_client, interaction);
        await _interactionService.ExecuteCommandAsync(ctx, _services);
    }

    private async Task OnMessageReceivedAsync(SocketMessage rawMessage)
    {
        if (rawMessage is not SocketUserMessage message) return;
        if (message.Author.IsBot) return;

        var argPos = 0;
        if (!(message.HasCharPrefix('!', ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos))) return;

        var ctx = new SocketCommandContext(_client, message);
        await _commandService.ExecuteAsync(ctx, argPos, _services);
    }
}

