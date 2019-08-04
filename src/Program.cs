using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MarksonDeckson.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace MarksonDeckson
{
    class Program
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IServiceProvider _services;
        private IConfiguration _config;

        public static void Main(string[] args)
        => new Program().MainAsync().GetAwaiter().GetResult();

        public Program()
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Info,
                MessageCacheSize = 50
            });

            _commands = new CommandService(new CommandServiceConfig
            {
                LogLevel = LogSeverity.Info,
                CaseSensitiveCommands = false,
            });

            _client.Log += Log;
            _commands.Log += Log;

            _services = ConfigureServices();
        }

        private async Task MainAsync()
        {
            ShowLogo();

            _config = BuildConfig();

            await InitCommands();

            await _client.LoginAsync(TokenType.Bot, _config["token"]);
            await _client.StartAsync();

            await Task.Delay(Timeout.Infinite);
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            var msg = arg as SocketUserMessage;
            if (msg == null) return;

            if (msg.Author.Id == _client.CurrentUser.Id || msg.Author.IsBot) return;

            int pos = 0;

            if (!(msg.HasCharPrefix('!', ref pos) ||
                msg.HasMentionPrefix(_client.CurrentUser, ref pos)) ||
                msg.Author.IsBot)
                return;

            var context = new SocketCommandContext(_client, msg);

            var result = await _commands.ExecuteAsync(context, pos, _services);

            if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
                await msg.Channel.SendMessageAsync(result.ErrorReason);
        }

        private async Task InitCommands()
        {
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

            _client.MessageReceived += HandleCommandAsync;
        }

        private static IServiceProvider ConfigureServices()
        {
            var map = new ServiceCollection();

            map.AddSingleton(new TranslationService());

            return map.BuildServiceProvider();
        }

        private IConfiguration BuildConfig()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config.json")
                .Build();
        }


        private static Task Log(LogMessage message)
        {
            switch (message.Severity)
            {
                case LogSeverity.Critical:
                case LogSeverity.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogSeverity.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogSeverity.Info:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case LogSeverity.Verbose:
                case LogSeverity.Debug:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    break;
            }
            Console.WriteLine($"{DateTime.Now,-19} [{message.Severity,8}] {message.Source}: {message.Message} {message.Exception}");
            Console.ResetColor();

            return Task.CompletedTask;
        }

        private void ShowLogo()
        {
            using (var file = File.Open("logo.txt", FileMode.OpenOrCreate))
            using (var reader = new StreamReader(file))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(reader.ReadToEnd());
                Console.ResetColor();
            }
        }
    }
}
