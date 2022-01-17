using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Orbital.Init;
using System;
using System.Threading.Tasks;

namespace Orbital.Commands
{
    [Group("Owner")]
    public class ManagementCommands : BaseCommandModule
    {
        [Command("restart")]
        [Description("Restart the discord bot")]
        [RequireOwner]
        public async Task Restart(CommandContext ctx)
        {
            ConsoleProgram.state = ConsoleProgram.StateEnum.Restarting;
            await VRC.StopAsync();
            await Bot.StopAsync();

            ConsoleProgram.state = ConsoleProgram.StateEnum.Running;

            Task.Run(async () => await VRC.StartAsync()).GetAwaiter();
            Task.Run(async () => await Bot.StartAsync()).GetAwaiter();
        }
        [Command("stop")]
        [Description("Stop the discord bot")]
        [RequireOwner]
        public async Task Stop(CommandContext ctx)
        {
            ConsoleProgram.state = ConsoleProgram.StateEnum.Stopped;
            await VRC.StopAsync();
            await Bot.StopAsync();
            Environment.Exit(0);
        }
    }

    [SlashCommandGroup("Owner", "Owner Command Module")]
    public class SlashManagementCommands : ApplicationCommandModule
    {
        [SlashCommand("restart", "Restart the discord bot")]
        [SlashRequireOwner]
        public async Task Restart(InteractionContext ctx)
        {
            ConsoleProgram.state = ConsoleProgram.StateEnum.Restarting;
            await VRC.StopAsync();
            await Bot.StopAsync();

            ConsoleProgram.state = ConsoleProgram.StateEnum.Running;

            Task.Run(async () => await VRC.StartAsync()).GetAwaiter();
            Task.Run(async () => await Bot.StartAsync()).GetAwaiter();
        }
       
        [SlashCommand("stop", "Stop the discord bot")]
        [SlashRequireOwner]
        public async Task Stop(InteractionContext ctx)
        {
            ConsoleProgram.state = ConsoleProgram.StateEnum.Stopped;
            await VRC.StopAsync();
            await Bot.StopAsync();
            Environment.Exit(0);
        }
    }
}
