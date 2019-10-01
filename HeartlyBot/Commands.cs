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
            char[] allowedChars = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };

            Dictionary<char, int> charDict = new Dictionary<char, int>();

            foreach (char c in text)
            {
                if (allowedChars.Contains(c))
                {
                    if (charDict.ContainsKey(c)) charDict[c]++;
                    else charDict.Add(c, 1);
                }
            }
            
            string[] words = File.ReadAllLines("commonwords.txt");
            List<string> possibleWords = new List<string>();
            foreach(string w in words)
            {
                var testDict = new Dictionary<char, int>(charDict);
                bool hasWord = true;
                foreach(char c in w)
                {
                    if (testDict.ContainsKey(c))
                    {
                        testDict[c]--;
                        if (testDict[c] <= 0) testDict.Remove(c);
                    }
                    else { hasWord = false; break; }
                }

                if (hasWord) possibleWords.Add(w);
            }

            List<string> anagrams = new List<string>();
            foreach(string w1 in possibleWords)
            {
                foreach(string w2 in possibleWords)
                {
                    foreach(string w3 in possibleWords)
                    {
                        string anagram = "";
                        var testDict = new Dictionary<char, int>(charDict);
                        var allChars = w1 + " " + w2 + " " + w3;

                        string potWord = "";
                        foreach (char c in allChars)
                        {
                            if (c == ' ')
                            {

                                anagram += potWord + ' ';
                                potWord = "";

                                if (testDict.Keys.Count() == 0) {
                                    if (!anagrams.Contains(anagram) || anagram == text)
                                        anagrams.Add(anagram);
                                    break;
                                }
                            }
                            else if (testDict.ContainsKey(c))
                            {
                                testDict[c]--;
                                if (testDict[c] <= 0) testDict.Remove(c);
                                potWord += c;
                            }
                            else break;
                        }
                    }
                }
            }

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