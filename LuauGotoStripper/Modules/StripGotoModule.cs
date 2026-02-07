using System.Text;
using Discord;
using Discord.Interactions;

namespace LuauGotoStripper.Modules;

public sealed class StripGotoModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly HttpClient _http;

    public StripGotoModule(HttpClient http)
    {
        _http = http;
    }

    [SlashCommand("stripgoto", "Upload a .lua file; I will strip goto/labels and return output.lua (ephemeral).")]
    public async Task StripGotoAsync(IAttachment file)
    {
        if (file == null)
        {
            await RespondAsync("Upload a .lua file.", ephemeral: true);
            return;
        }

        if (file.Size > 25 * 1024 * 1024)
        {
            await RespondAsync("File too large.", ephemeral: true);
            return;
        }

        await DeferAsync(ephemeral: true);

        byte[] inputBytes;
        try
        {
            using var resp = await _http.GetAsync(file.Url);
            resp.EnsureSuccessStatusCode();
            inputBytes = await resp.Content.ReadAsByteArrayAsync();
        }
        catch (Exception ex)
        {
            await FollowupAsync($"Failed to download file: {ex.Message}", ephemeral: true);
            return;
        }

        string inputText;
        try
        {
            inputText = Encoding.UTF8.GetString(inputBytes);
        }
        catch
        {
            inputText = Encoding.Latin1.GetString(inputBytes);
        }

        var cleaned = LuauGotoStripper.Cleaner.Clean(inputText, out var removedLines);
        var outBytes = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false).GetBytes(cleaned);
        await using var ms = new MemoryStream(outBytes);

        await FollowupWithFileAsync(
            ms,
            "output.lua",
            text: $"Removed {removedLines} line(s).",
            ephemeral: true);
    }
}
