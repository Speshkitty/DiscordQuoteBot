using Discord;
using Discord.Commands;
using Discord.Commands.Builders;
using Discord.Interactions;
using Discord.Net;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.ComponentModel.Design;
using System.Reflection;
using System.Windows.Input;

namespace DiscordQuoteBot
{
    internal class Program
    {
        private static DiscordSocketClient? _client;
        private static ulong _testGuildId = 55333533735985152;
        private static InteractionService? _interactionService;
        private static IServiceProvider? _serviceProvider;
        private static CommandHandler? _commandHandler;
        private static Data? _data;

        public static async Task Main()
        {
            _serviceProvider = CreateProvider();
            _client = _serviceProvider.GetRequiredService<DiscordSocketClient>();
            _data = _serviceProvider.GetRequiredService<Data>();
            _data.LoadData();
            _data.SaveData();

            string oldQuoteFile = "quotes.json";

            if (File.Exists(oldQuoteFile))
            {
                Dictionary<ulong, List<Quote>> loadedData = JsonConvert.DeserializeObject< Dictionary<ulong, List<Quote>>>(File.ReadAllText(oldQuoteFile));

                foreach(var v in loadedData)
                {
                    var d = _data.GetDataForServer(v.Key);
                    foreach (var q in v.Value)
                    {
                        d.QuoteList.Add(q);
                    }
                }

                _data.SaveData();
                File.Delete(oldQuoteFile);
            }

            _client.Log += Log;
            _client.Ready += client_Ready;


            var token = "";
            try
            {
                token = File.ReadAllText("token.txt");
            }
            catch (Exception ex) when (
                ex is IOException ||
                ex is FileNotFoundException ||
                ex is DirectoryNotFoundException)
            {
                await Log(new LogMessage(LogSeverity.Critical, "Startup", "Token not found!", ex));
                return;
            }

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();


            await Task.Delay(-1);
        }

        private async static Task client_Ready()
        {
            _interactionService = new InteractionService(_client!.Rest);
            _commandHandler = new CommandHandler(_client!, _interactionService!, _serviceProvider!);
            await _interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), _serviceProvider);

            if (IsDebug())
            {
                // this is where you put the id of the test discord guild
                Console.WriteLine($"In debug mode, adding commands to {_testGuildId}...");
                await _interactionService!.RegisterCommandsToGuildAsync(_testGuildId);
            }
            else
            {
                // this method will add commands globally, but can take around an hour
                await _interactionService!.RegisterCommandsGloballyAsync(true);
            }

            _client!.InteractionCreated += async interaction =>
            {
                var scope = _serviceProvider!.CreateScope();
                var ctx = new SocketInteractionContext(_client, interaction);
                await _interactionService.ExecuteCommandAsync(ctx, scope.ServiceProvider);
            };


        }

        static IServiceProvider CreateProvider()
        {
            var config = new DiscordSocketConfig()
            {
                
            };

            var collection = new ServiceCollection()
            .AddSingleton(config)
            .AddSingleton<DiscordSocketClient>()
            .AddSingleton<Data>()
            .AddSingleton<CommandHandler>();

            return collection.BuildServiceProvider();
        }

        private static Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        static bool IsDebug()
        {
#if DEBUG
            return true;
#else
            return false;
#endif
        }
    }
}
