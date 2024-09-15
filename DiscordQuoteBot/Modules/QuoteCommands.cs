using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Text;

namespace DiscordQuoteBot.Modules
{
    [Group("quote", "Commands relating to quotes")]
    public class QuoteCommands : InteractionModuleBase<SocketInteractionContext>
    {
        public Data? Data { get; set; }
        public DiscordSocketClient? Client { get; set; }

        [SlashCommand("add", "Adds a quote")]
        public async Task AddQuote(string quote)
        {
            var list = Data!.GetDataForServer(Context.Guild.Id).QuoteList;
            list.Add(
                new Quote()
                {
                    AddedBy = Context.User.Id,
                    QuoteText = quote
                });
            Data.SaveData();
            await RespondAsync($"Added quote #{list.Count} `{quote}`");
        }

        [SlashCommand("info", "Provides quote information")]
        public async Task Info(int quoteNum = 0)
        {
            var list = Data!.GetDataForServer(Context.Guild.Id).QuoteList;
            if (quoteNum == 0)
            {
                //General information
                await RespondAsync($"{list.Count} quotes in database", ephemeral: true);
            }
            else
            {
                quoteNum--;
                if(quoteNum < 0 || quoteNum > list.Count)
                {
                    await RespondAsync($"Quote Number should be between 1 and {list.Count}", ephemeral: true);
                    return;
                }
                IUser adder = await Client!.GetUserAsync(list[quoteNum].AddedBy);
                await RespondAsync(
                    $"**Quote Number {quoteNum+1}**\n" +
                    $"Added by {adder.Mention}\n" + 
                    $"On {list[quoteNum].TimeAdded}",
                    ephemeral: true);
            }
        }
        [SlashCommand("list", "Lists up to 10 quotes per page")]
        public async Task List(int pageNum = 1)
        {
            pageNum--;
            StringBuilder sb = CreateQuoteList(Context.Guild.Id, pageNum);

            if (BuildCompsForListPage(Context.Guild.Id, pageNum, out ComponentBuilder builder))
            {
                await RespondAsync(sb.ToString(), ephemeral: true, components: builder.Build());
            }
            else
            {
                await RespondAsync(sb.ToString(), ephemeral: true);
            }
            Client!.ButtonExecuted += Client_ButtonExecuted;
        }

        private bool BuildCompsForListPage(ulong? GuildId, int pageNum, out ComponentBuilder builder)
        {
            builder = new ComponentBuilder();
            bool doButtons = false;
            
            var list = Data!.GetDataForServer(Context.Guild.Id).QuoteList;
            int maxPage = list.Count / 10;

            if (pageNum != 0)
            {
                builder = builder.WithButton("Previous", $"quote-list-button-{pageNum - 1}");
                doButtons = true;
            }
            if (pageNum < maxPage)
            {
                builder = builder.WithButton("Next", $"quote-list-button-{pageNum + 1}");
                doButtons = true;
            }

            return doButtons;
        }
        private StringBuilder CreateQuoteList(ulong? GuildId, int pageNum)
        {
            var list = Data!.GetDataForServer(Context.Guild.Id).QuoteList;
            int maxPage = list.Count / 10;

            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"Quote page {pageNum + 1} of {maxPage + 1}");
            int remaining = list.Count - (pageNum * 10);

            for (int i = 0; i < Math.Min(10, remaining); i++)
            {
                int quoteNum = pageNum * 10 + i;
                sb.AppendLine($"{quoteNum + 1}: `{list[quoteNum].QuoteText}`");
            }

            return sb;
        }

        private Task Client_ButtonExecuted(SocketMessageComponent component)
        {
            if (component.Data.CustomId.StartsWith("quote-list-button-"))
            {
                int pageToLoad = int.Parse(component.Data.CustomId.Substring(component.Data.CustomId.LastIndexOf('-') + 1));
                if (BuildCompsForListPage(component.GuildId, pageToLoad, out ComponentBuilder builder))
                {
                    component.UpdateAsync(comp =>
                    {
                        comp.Content = CreateQuoteList(component.GuildId, pageToLoad).ToString();
                        comp.Components = builder.Build();
                    });
                }
                else {
                    component.UpdateAsync(comp =>
                    {
                        comp.Content = CreateQuoteList(component.GuildId, pageToLoad).ToString();
                    });
                }
            }
            return Task.CompletedTask;
        }

        [SlashCommand("say", "Says a quote with optional ID")]
        public async Task SayQuote(int quoteNum = 0)
        {
            var list = Data!.GetDataForServer(Context.Guild.Id).QuoteList;
            if(quoteNum > list.Count)
            {
                await RespondAsync($"Number too high - max quote number is {list.Count}", ephemeral: true);
                return;
            }
            if(quoteNum == 0)
            {
                quoteNum = new Random().Next(list.Count);
            }
            else
            {
                quoteNum--;
            }

            await RespondAsync($"{quoteNum + 1}: `{list[quoteNum].QuoteText}`");
        }
    }
}
