using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace HeartlyBot
{
    public class Commands : ModuleBase
    {
        [Command("help"), Summary("Displays commands and descriptions.")]
        public async Task Help()
        {
            JEmbed emb = new JEmbed();
            emb.Author.Name = "Commands";
            emb.ColorStripe = GetColor(Context.User);

            foreach (CommandInfo command in Bot.commands.Commands)
            {
                emb.Fields.Add(new JEmbedField(x =>
                {
                    string header = "<3" + command.Name;
                    foreach (ParameterInfo parameter in command.Parameters)
                    {
                        header += " [" + parameter.Name + "]";
                    }
                    x.Header = header;
                    x.Text = command.Summary;
                }));
            }
            await Context.Channel.SendMessageAsync("", embed: emb.Build());
        }
    
        [Command("ping"), Summary("Pong!")]
        public async Task Ping()
        {
            await ReplyAsync("Pong! :heart:");
        }

        [Command("anagram"), Summary("Rearrange text to make... Different text!!!")]
        public async Task Anagram([Remainder] string text)
        {
            text = text.ToLower();
            var hashedDictionary = new Dictionary<string, int>();
            var words = File.ReadAllLines("commonwords.txt");
            foreach(string word in words)
            {
                try
                {
                    var alphabetical = word.OrderBy(x => x);
                    string alph = "";
                    foreach (char c in alphabetical) alph += c;
                    hashedDictionary.Add(alph, Hash(alph));
                }
                catch (Exception)
                {
                    //Console.WriteLine("bah bah bah");
                }
            }


            string[] anagrams = new string[0];
            int anagramCount = anagrams.Count();
            int loopCount;
            if (anagramCount > 10) loopCount = 10;
            else loopCount = anagramCount;

            string aList = "```\n";
            for (int i = 0; i < loopCount; i++) aList += anagrams[i] + "\n";
            aList += "```";

            if (anagramCount > 0) await ReplyAsync(anagramCount + " anagram(s) found: " + aList);
            else await ReplyAsync("No anagrams found.");
        }
        
        public static int Hash(string str)
        {
            int total = 0;
            foreach (char c in str) total += c;
            return total;
        }

        public static Color GetColor(IUser User)
        {
            var user = User as IGuildUser;
            if (user != null)
            {
                if (user.RoleIds.ToArray().Count() > 1)
                {
                    var role = user.Guild.GetRole(user.RoleIds.ElementAtOrDefault(1));
                    return role.Color;
                }
                else return Constants.Colours.DEFAULT_COLOUR;
            }
            else return Constants.Colours.DEFAULT_COLOUR;
        }
    }

}