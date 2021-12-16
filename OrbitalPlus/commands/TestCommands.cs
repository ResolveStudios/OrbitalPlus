﻿using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orbital.Data;

namespace Orbital.Commands
{
    public class TestCommands : BaseCommandModule
    {
        [Command("test")]
        public async Task Test(CommandContext ctx, string typeOfTest = null)
        {
            if (typeOfTest == "alert")
            {
                await ctx.Channel.SendMessageAsync("Generating a test lecture for testing purposes... will ping a Lecture Alert when ready.\nDon't forget to remove this test lecture once pinged with `LectureAlert Resovle`");
                Models.Lecture testLecture = new Models.Lecture()
                {
                    Day = DateTime.Now,
                    IsDropIn = false,
                    Groups = "Test group data",
                    IsTutorial = false,
                    Lecturer = "Dr Test",
                    ModuleName = "Test module",
                    StartTime = DateTime.Now.TimeOfDay.Add(new TimeSpan(0, 30, 0))
                };
                TimetableData.Timetable.Lectures.Add(testLecture);
            }
            else if (typeOfTest == "resolve")
            {
                Models.Lecture testLecture = TimetableData.Timetable.Lectures.Find(l => l.ModuleName.Equals("Test module"));
                await ctx.Channel.SendMessageAsync($"Successfully removed {testLecture.ToString()}.");
                TimetableData.Timetable.Lectures.Remove(testLecture);
            }
        }
    }
}
