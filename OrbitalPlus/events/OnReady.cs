using DSharpPlus;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using System.Threading;
using Orbital.Models;
using System.IO;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Orbital.Data;
using System.Linq;
using Orbital.Init;
using System.Drawing;

namespace Orbital.Events
{
    public static class OnReady
    {
        private static readonly DiscordClient ctx = Init.Bot.ctx;
        public static readonly string READ_WRITE_PATH = @$"{Directory.GetDirectoryRoot(Directory.GetCurrentDirectory())}";
        public static Timetable Timetable;
        public static string AssignmentsDeadlineString = "";
        public static List<Assignment> AssDueList = new List<Models.Assignment>();
        public static Task OnTrigger(DiscordClient sender, DSharpPlus.EventArgs.ReadyEventArgs e)
        {
            Timetable = new Timetable();
            Task.Run(async () => await LectureNotification(new CancellationToken()));

            DateTime postStartUpTime = DateTime.UtcNow;

            PrintInit();
             Debug.Log($"[Startup time: {(postStartUpTime - ConsoleProgram.preStartUpTime).TotalSeconds.ToString()} seconds] ", Color.Blue, false);
            
            var status = new DiscordActivity("Orbital+ VRC Activity", ActivityType.Watching);
            ctx.UpdateStatusAsync(status);
            

            Debug.Log("\n--------------------- All Systems Online ---------------------", Color.Blue, false);

             Debug.Log("Building Models...", Color.White, false);
            BuildModels();
            if (TopicData.CsTopics != null)
            {
                Debug.Log(TopicData.CsTopics.Subject.ToString() + " Data Model exists.", Color.Yellow, false);
            } else
            {
                Debug.Log("COMPUTER SCIENCE Data Model does not exist.", Color.Red, false);
            }

            if (Data.TopicData.SoftEngTopics != null)
            {
                Debug.Log(TopicData.SoftEngTopics.Subject.ToString() + " Data Model exists.", Color.White, false);
            } else
            {
                Debug.Log("SOFTWARE ENGINEERING Data Model does not exist.", Color.Red, false);
            }

            Debug.Log("Models successfully built!", Color.Green, false);
            Debug.Log("Listening...\n", Color.DarkCyan, false);

            foreach (var t in Data.TopicData.CsTopics.Topics)
            {
                foreach (var a in t.Assignments)
                {
                    if (a.Deadline.DayOfYear > DateTime.Now.DayOfYear)
                    {
                        AssignmentsDeadlineString += $"\n __**{t.Name}**__: \n*{a.AssignmentTitle}* - Worth: **{a.Percentage}%** \n Due in: **{a.Deadline.DayOfYear - DateTime.Now.DayOfYear} days**\n-----------------------\n";
                    }
                }
            }
            return Task.CompletedTask;
        }

        private static async void BuildModels()
        {
            Data.TopicData.CsTopics = new TopicList(Subject.COMPUTER_SCIENCE);
            if (!File.Exists(READ_WRITE_PATH + @"CsTopicData.json"))
            {
                 Debug.Log("Json data does not exist... Generating the data now...", Color.White, false);
                foreach(var topic in Data.TopicData.CsTopics.Topics)
                {
                    topic.BuildAssignments();
                }
                var s = JsonConvert.SerializeObject(Data.TopicData.CsTopics);
                await File.WriteAllTextAsync(READ_WRITE_PATH + @"CsTopicData.json", s);
                
                foreach (var module in Data.TopicData.CsTopics.Topics)
                {
                     Debug.Log($"\t{module.Name}: ", Color.Green, false);

                    foreach (var assignment in module.Assignments)
                    {
                         Debug.Log($"\t\t {assignment.AssignmentTitle} - {assignment.Percentage}% - [{assignment.Semester.ToString().ToUpper()} SEMESTER]\n" +
                            $"\t\t Deadline: {assignment.Deadline.ToShortDateString()} | Feedback {assignment.FeedbackReturn.ToShortDateString()}\n", Color.White);
                    }
                }

                Debug.Log("\n\nJson sample data successfully generated and saved.\n\n", Color.Green); 
            } 
            else
            {
                Debug.Log("Json file found for data... importing now... ", Color.White, false);
                TopicData.CsTopics = JsonConvert.DeserializeObject<TopicList>(File.ReadAllText(READ_WRITE_PATH + @"CsTopicData.json"));
                Debug.Log("All data has successfully been imported!", Color.Green, false);
            }
        }

        private static bool messageSent = false;
        private static Lecture currentLecture;
        private static async Task LectureNotification(CancellationToken cancellationToken)
        {
            while (true)
            {
                Random rand = new Random();
                int recallTime = rand.Next(1500, 5000);
                if(!messageSent)
                {
                    foreach(var lecture in Timetable.Lectures)
                    {
                        if (lecture.Day.DayOfWeek == DateTime.Now.DayOfWeek)
                        {
                            if ((lecture.StartTime - (new TimeSpan(0, 30, 0)) <= DateTime.Now.TimeOfDay)) // Checks to see if current time is greater than the announcement gap
                            {
                                var minutesRemaining = (lecture.StartTime - DateTime.Now.TimeOfDay).Minutes;
                                if ((minutesRemaining) > 0 && (minutesRemaining) < 30)
                                {
                                    currentLecture = lecture;
                                    // gets the mention tag from guild roles instead of mentioning explicity in a string.
                                    await ctx.GetChannelAsync(801886321395761152).Result.SendMessageAsync($"{ctx.GetChannelAsync(801191647274467328).Result.Guild.GetRole(801920056246272082).Mention} \n **{lecture.ModuleName}** is about to start in **{(lecture.StartTime - DateTime.Now.TimeOfDay).Minutes}** minutes!\n**Groups:** *{lecture.Groups}*\n**Lecturer:** *{lecture.Lecturer}* \n {(lecture.IsTutorial ? "** --- This is a tutorial ---**" : "**--- This is a lecture ---**")} \n_______________________________________________");
                                    messageSent = true;
                                }
                            }
                        }
                    } 
                } else
                {
                    var checkMinutes = (currentLecture.StartTime - DateTime.Now.TimeOfDay).Minutes;

                    if (checkMinutes <= 0)
                    {
                        messageSent = false;
                        currentLecture = null;
                    }
                }
                await Task.Delay(recallTime * 60, cancellationToken);
                if (cancellationToken.IsCancellationRequested)
                    break;
            }
        }

        private static void PrintInit()
        {
            string onLoadGreeting = @"
                                   ____       _     _ _        _       
                                  / __ \     | |   (_) |      | |  _   
                                 | |  | |_ __| |__  _| |_ __ _| |_| |_ 
                                 | |  | | '__| '_ \| | __/ _` | |_   _|
                                 | |__| | |  | |_) | | || (_| | | |_|  
                                  \____/|_|  |_.__/|_|\__\__,_|_|      
";
            Debug.Log(onLoadGreeting, Color.Magenta);
            Debug.Log("                     Made with <3 by OkashiKami | https://github.com/OkashiKami/Orbial-", Color.Yellow);
        }
    }
}
