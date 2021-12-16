using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Orbital.Data;
using Orbital.Init;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orbital.Commands
{
    [Group("vrchat")]
    public class VRChatCommands : BaseCommandModule
    {
        [Command("set-vrcname")]
        [Priority(1)]
        [Description("Sets your vrchat name in the discord server")]
        public async Task SetVRCName(CommandContext ctx, [RemainingText, Description("your vrchat name")] string vrchatname = null)
        {
            var memberManager = Resoruces.Get<MemberManager>();
            memberManager.RegisterUser(ctx.Member.Username, vrchatname);
            var role = ctx.Guild.Roles.FirstOrDefault(x => x.Value.Name == "VRCLinked").Value;
            if (!ctx.Member.Roles.Contains(role))
                await ctx.Member.GrantRoleAsync(role);
            await ctx.Message.DeleteAsync();
        }

        [Command("set-vrcname")]
        [Priority(0)]
        [Description("Sets the vrchat name of another user in the discord server")]
        public async Task SetVRCName(CommandContext ctx, [Description("The member you want to register")] DiscordMember member, [RemainingText, Description("their vrchat name")] string vrchatname = null)
        {
            var members = Resoruces.Get<MemberManager>();
            members.RegisterUser(member.Username, vrchatname);
            var role = ctx.Guild.Roles.FirstOrDefault(x => x.Value.Name == "VRCLinked").Value;
            if (!ctx.Member.Roles.Contains(role))
                await ctx.Member.GrantRoleAsync(role);
            await ctx.Message.DeleteAsync();
        }
        [Command("whoami")]
        [Description("Give you information about your vrc account")]
        public async Task WhoAmI(CommandContext ctx)
        {

        }
    } 
}
