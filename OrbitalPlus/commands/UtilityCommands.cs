using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orbital.Commands
{
    [Group("utilities")]
    public class UtilityCommands : BaseCommandModule
    {
        [Command("ping")]
        [Description("Will test the response time of a request to the bot.")]
        [Cooldown(maxUses: 1, resetAfter: 3, bucketType: CooldownBucketType.User)]
        public async Task Ping(CommandContext ctx)
        {
            DateTime before = DateTime.UtcNow;
            await ctx.Channel.SendMessageAsync("Pong :ping_pong:");
            DateTime after = DateTime.UtcNow;
            await ctx.Channel.SendMessageAsync(((after - before).Milliseconds / 10).ToString() + "ms response time. :chart_with_upwards_trend:");
        }

        [Cooldown(maxUses: 1, resetAfter: 5, bucketType: CooldownBucketType.Channel)]
        [Aliases("commands", "github", "")]
        [Command("help")]
        public async Task Help(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync("You can find all the commands on alta's github reposoitory: https://github.com/OkashiKami/Orbial-");
        }

        [Command("date")]
        public async Task Date(CommandContext ctx)
        {
            TimeSpan timeSpan = new TimeSpan(DateTime.Now.TimeOfDay.Hours, DateTime.Now.TimeOfDay.Minutes, DateTime.Now.TimeOfDay.Seconds);
            DateTime dateinfo = DateTime.Today.Add(timeSpan);


            await ctx.Channel.SendMessageAsync($"The day is **{dateinfo.DayOfWeek}** (*{dateinfo.ToShortDateString()}*) and it is **{dateinfo.ToString("hh:mmtt")}**. :stopwatch:");
        }

        [Command("alerts")]
        public async Task Alerts(CommandContext ctx, string unsubscribe = null)
        {
            if (unsubscribe == "unsubscribe")
            {
                if (ctx.Member.Roles.Any(r => r.Name.Equals("LectureAlerts")))
                {
                    var role = ctx.Member.Roles.FirstOrDefault(r => r.Name.Equals("LectureAlerts"));
                    await ctx.Member.RevokeRoleAsync(role);
                    await ctx.Channel.SendMessageAsync($"Ok {ctx.Member.Mention}, you wont get any more notifications :ok_hand:");
                }
                else
                {
                    await ctx.Channel.SendMessageAsync($"Erm... You weren't subscribed in the first place {ctx.Member.Mention} :thinking:");
                }
            }
            else if (unsubscribe == null)
            {
                await ctx.Channel.SendMessageAsync("You can subscribe to lecture alerts and I will ping you roughly 30 minutes before a lecture or tutorial session is about to start!\n"
                    + "**Usage:**\n`!hb alerts subscribe` - Gives you the role LectureAlerts\n" +
                    "`!hb alerts unsubscribe` - Removes you from the role LectureAlerts and prevents notifications");
            }
            else if (unsubscribe == "subscribe")
            {
                if (ctx.Member.Roles.Any(r => r.Name.Equals("LectureAlerts")))
                {
                    await ctx.Channel.SendMessageAsync($"You are already subscribed {ctx.Member.Mention}!");
                }
                else
                {
                    await ctx.Member.GrantRoleAsync(ctx.Guild.GetRole(801888523538923520));
                    await ctx.Channel.SendMessageAsync($"All set {@ctx.Member.Mention}! You will get notified of future lectures / tutorials!");
                }
            }
        }


        [Command("deadlines")]
        public async Task Deadlines(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync(Events.OnReady.AssignmentsDeadlineString);
        }
    }
    
    [SlashCommandGroup("utilities", "Utiliteies Command Module")]
    public class SlashUtilityCommands : ApplicationCommandModule
    {
        [SlashCommand("ping", "Will test the response time of a request to the bot.")]
        public async Task Ping(InteractionContext ctx)
        {
            DateTime before = DateTime.UtcNow;
            await ctx.Channel.SendMessageAsync("Pong :ping_pong:");
            DateTime after = DateTime.UtcNow;
            await ctx.Channel.SendMessageAsync(((after - before).Milliseconds / 10).ToString() + "ms response time. :chart_with_upwards_trend:");
        }

        [SlashCommand("help", "default help")]
        public async Task Help(InteractionContext ctx)
        {
            await ctx.Channel.SendMessageAsync("You can find all the commands on alta's github reposoitory: https://github.com/OkashiKami/Orbial-");
        }

        [SlashCommand("copy-role", "Copy the specified role")]
        [SlashRequireOwner]
        public async Task CopyRole(InteractionContext ctx, [Option("DiscordRole", "The role that you would like to copy")] DiscordRole role)
        {
            var _role = ctx.Guild.GetRole(role.Id);
            await ctx.Guild.CreateRoleAsync($"{_role.Name} Copy", _role.Permissions, _role.Color, _role.IsHoisted, _role.IsMentionable);
            await ctx.CreateResponseAsync("Role has been duplicated");
        }
    }
}
