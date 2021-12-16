using Orbital.Data;
using Orbital.Init;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VRChat.API.Api;
using VRChat.API.Client;
using VRChat.API.Model;

namespace Orbital
{
    internal class VRC
    {
        private static IVRChat _vrchat;

        internal static void Init()
        {
            if (string.IsNullOrEmpty(Resoruces.Get<Settings>().vrc_username) || string.IsNullOrEmpty(Resoruces.Get<Settings>().vrc_password) || string.IsNullOrEmpty(Resoruces.Get<Settings>().vrc_apikey))
            {
                Debug.Log($"Error({Errors.NoVRCInfo}) {Errors.GetReason(Errors.NoVRCInfo)}");
                Errors.SetError();
                return;
            }

            _vrchat = new VRChatClientBuilder()
              .WithCredentials(Resoruces.Get<Settings>().vrc_username, Resoruces.Get<Settings>().vrc_password)
              .WithApiKey(Resoruces.Get<Settings>().vrc_apikey)
              .Build();
        }

        public static async Task StartAsync()
        {
            bool success = await _vrchat.TryLoginAsync();
            if (success)
            {
                Debug.Log("Was able to login successfully!");
                var notifications = await _vrchat.Notifications.GetNotificationsAsync();
                foreach (var notification in notifications)
                {
                    switch(notification.Type)
                    {
                        case NotificationType.FriendRequest: await _vrchat.Notifications.AcceptFriendRequestAsync(notification.Id); return;
                        default:
                            Debug.Log(notification.ToJson());
                            break;
                    }
                }

            }
        }

        private static async Task<User> GetUserById(string userid)
        {
            try
            {
                Debug.Log("Getting Other Player Info");
                User OtherUser = await _vrchat.Users.GetUserAsync(userid);
                if (OtherUser == null) return default;
                Debug.Log($"Found user {OtherUser.DisplayName}, joined {OtherUser.DateJoined}");
                return Task.FromResult(OtherUser).Result;
            }
            catch (ApiException e)
            {
                Console.WriteLine($"Exception when calling API: {e.Message}");
                Console.WriteLine($"Status Code: {e.ErrorCode}");
                Console.WriteLine(e.ToString());
                return default;
            }
        }
        private static async Task<World> GetWorldById(string worldid)
        {
            try
            {
                Debug.Log("Getting World Info");
                World World = await _vrchat.Worlds.GetWorldAsync(worldid);
                if (World == null) return default;
                Debug.Log($"Found world {World.Name}, visits: {World.Visits}");
                return Task.FromResult(World).Result;
            }
            catch (ApiException e)
            {
                Console.WriteLine($"Exception when calling API: {e.Message}");
                Console.WriteLine($"Status Code: {e.ErrorCode}");
                Console.WriteLine(e.ToString());
                return default;
            }
        }
    }
}