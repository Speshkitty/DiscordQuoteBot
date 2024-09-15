using Discord.Interactions;

namespace DiscordQuoteBot.Modules
{
    public class TestingCommands : InteractionModuleBase<SocketInteractionContext>
    {
#if DEBUG
        [SlashCommand("echo", "Echos the input")]
        public async Task Echo(string input)
        {
            await RespondAsync("Respond: " + input, ephemeral: true);
        }
#endif
    }
}
