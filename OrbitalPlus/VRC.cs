using Orbital.Data;
using Orbital.Init;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VRChat.API.Api;
using VRChat.API.Client;
using VRChat.API.Model;

namespace Orbital
{
    internal class VRC
    {
        private static IVRChat _vrchat;
        
        public static async Task InitAsync()
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
            await Task.Delay(1);
        }


        public static async Task StartAsync()
        {
            Debug.Log("Starting Discord Client...");
            bool success = await _vrchat.TryLoginAsync();
            if (success)
            {

                Task.Run(async () => await VRCStartAsync());
                Task.Run(async () => await VRCLoop());
                var s  = await _vrchat.Instances.SendSelfInviteAsync("wrld_b096eca5-e552-40c7-8db5-7ad74cc8100e", "Events");
                Debug.Log("Orbital+ Was able to login to VR Chat successfully!");
            }
        }

        private static async Task VRCStartAsync()
        {
            await Task.Run(async () => await CheckNotification());
            await Task.Delay(1);
        }

        private static async Task VRCLoop()
        {
            while(_vrchat.IsLoggedIn)
            {

            }
            await Task.Delay(1);
        }

        private static async Task CheckNotification()
        {
            Debug.Log("Checking notification...");
            var notifications = await _vrchat.Notifications.GetNotificationsAsync();
            if (notifications == null) return;
            foreach (var notification in notifications)
            {
                switch (notification.Type)
                {
                    case NotificationType.FriendRequest: await _vrchat.Notifications.AcceptFriendRequestAsync(notification.Id); return;
                    case NotificationType.RequestInvite: _vrchat.Invites.InviteUser(null, new InviteRequest() { InstanceId = "" }); break;
                    case NotificationType.Invite:


                        notification.Seen = true;
                        break;
                    default:
                        Debug.Log(notification.ToJson());
                        break;
                }
            }
            Debug.Log("Notification checking done!");
            await Task.Delay(5000);
            if (_vrchat.IsLoggedIn)
                await CheckNotification();
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
    }
}