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
            if (string.IsNullOrEmpty(Resources.Get<Settings>().vrc_username) || string.IsNullOrEmpty(Resources.Get<Settings>().vrc_password) || string.IsNullOrEmpty(Resources.Get<Settings>().vrc_apikey))
            {
                Debug.Log($"Error({Errors.NoVRCInfo}) {Errors.GetReason(Errors.NoVRCInfo)}");
                Errors.SetError();
                return;
            }

            _vrchat = new VRChatClientBuilder()
              .WithCredentials(Resources.Get<Settings>().vrc_username, Resources.Get<Settings>().vrc_password)
              .WithApiKey(Resources.Get<Settings>().vrc_apikey)
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

        public static async Task<User> GetUserByIdAsync(string userid)
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
                 Debug.Log($"Exception when calling API: {e.Message}");
                 Debug.Log($"Status Code: {e.ErrorCode}");
                 Debug.Log(e.ToString());
                return default;
            }
        }
        public static async Task<User> GetUserByNameAsync(string username)
        {
            try
            {
                Debug.Log("Getting Other Player Info");
                var OtherUser = await _vrchat.Users.GetUserByNameAsync(username);
                if (OtherUser == null) return default;
                Debug.Log($"Found user {OtherUser.DisplayName}, joined {OtherUser.DateJoined}");
                return Task.FromResult(OtherUser).Result;
            }
            catch (ApiException e)
            {
                Debug.Log($"Exception when calling API: {e.Message}");
                Debug.Log($"Status Code: {e.ErrorCode}");
                 Debug.Log(e.ToString());
                return default;
            }
        }
        public static async Task<World> GetWorldByIdAsync(string worldid)
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
                 Debug.Log($"Exception when calling API: {e.Message}");
                 Debug.Log($"Status Code: {e.ErrorCode}");
                 Debug.Log(e.ToString());
                return default;
            }
        }
        public static async Task<User> SearchAsync(string displayname)
        {
            try
            {
                List<LimitedUser> users = await _vrchat.Users.SearchUsersAsync(displayname, n: 1);
                if (users.Count <= 0) return default;
                var user =  await GetUserByNameAsync(users[0].Username);
                return Task.FromResult(user).Result;
            }
            catch (ApiException e)
            {
                Debug.Log($"Exception when calling API: {e.Message}");
                Debug.Log($"Status Code: {e.ErrorCode}");
                Debug.Log(e.ToString());
                return default;
            }
        }
    }
}