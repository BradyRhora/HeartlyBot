using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Reflection;

namespace HeartlyBot
{
    public class Bot
    {
        static void Main(string[] args) => new Bot().Run().GetAwaiter().GetResult();

        #region Vars
        public static DiscordSocketClient client;
        public static CommandService commands;
        #endregion

        public async Task Run()
        {
            Start:
            try
            {
                Console.WriteLine("Welcome, Brady. Initializing Bot...");
                client = new DiscordSocketClient();
                Console.WriteLine("Client Initialized.");
                commands = new CommandService();
                Console.WriteLine("Command Service Initialized.");
                await InstallCommands();
                Console.WriteLine("Commands Installed, logging in.");
                await client.LoginAsync(TokenType.Bot, /*INSERT TOKEN*/);
                Console.WriteLine("Successfully logged in!");
                // Connect the client to Discord's gateway
                await client.StartAsync();
                Console.WriteLine("Bot successfully intialized\n");
                // Block this task until the program is exited.
                await Task.Delay(-1);
            }
            catch (Exception e)
            {
                Console.WriteLine("\n==========================================================================");
                Console.WriteLine("                                  ERROR                        ");
                Console.WriteLine("==========================================================================\n");
                Console.WriteLine($"Error occured in {e.Source}");
                Console.WriteLine(e.Message);
                Console.WriteLine(e.InnerException);

                Again:

                Console.WriteLine("Would you like to try reconnecting? [Y/N]");
                var input = Console.Read();

                if (input == 121) { Console.Clear(); goto Start; }
                else if (input == 110) Environment.Exit(0);

                Console.WriteLine("Invalid input.");
                goto Again;
            }
        }

        public async Task InstallCommands()
        {
            client.MessageReceived += HandleCommand;
            client.Ready += HandleReady;
            await commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        //Code that runs when bot recieves a message
        public async Task HandleCommand(SocketMessage messageParam)
        {
            var message = messageParam as SocketUserMessage;
            if (message == null) return;
            int argPos = 0;

            if (message.HasCharPrefix('+', ref argPos))
            {

                var context = new CommandContext(client, message);
                var result = await commands.ExecuteAsync(context, argPos);
                if (!result.IsSuccess)
                    Console.WriteLine(result.ErrorReason);
            }
            else return;
        }

        public async Task HandleReady()
        {
            foreach (IGuild g in client.Guilds)
            {
                Console.WriteLine(g.Name + " " + g.Id);
                var invites = await g.GetInvitesAsync();

                Console.WriteLine(invites.FirstOrDefault());
                Console.Beep();
            }
        }

    }



}
