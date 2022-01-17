using Orbital.Data;
using Orbital.Init;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
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

        private static string worldid => "wrld_b096eca5-e552-40c7-8db5-7ad74cc8100e";
        private static string instanceID => "OrbitalPlus";//new Random().Next(99999, 999999999).ToString();

        private static async Task InitAsync()
        {
            if (string.IsNullOrEmpty(Resources.Get<Settings>().vrc_username) || string.IsNullOrEmpty(Resources.Get<Settings>().vrc_password) || string.IsNullOrEmpty(Resources.Get<Settings>().vrc_apikey))
            {
                Debug.Log($"Error({Errors.NoVRCInfo}) {Errors.GetReason(Errors.NoVRCInfo)}", header: true);
                Errors.SetError();
                return;
            }
            _vrchat = new VRChatClientBuilder()
              .WithCredentials(Resources.Get<Settings>().vrc_username, Resources.Get<Settings>().vrc_password)
              .WithApiKey(Resources.Get<Settings>().vrc_apikey)
              .Build();
            await Task.CompletedTask;
        }


        public static async Task StartAsync()
        {
            await InitAsync();
            if (Errors.has) return;
            Debug.Log("Starting VRChat API Client...", header: true);
            bool success = await _vrchat.TryLoginAsync();
            if (success)
            {

                Task.Run(async () => await CheckNotification()).GetAwaiter();
                Task.Run(async () => await VRCLoop()).GetAwaiter();
                Debug.Log("Orbital+ Was able to login to VR Chat successfully!", header: true);
                var curUser = await _vrchat.Authentication.GetCurrentUserAsync();
                curUser.HasLoggedInFromClient = true;
                curUser.State = UserState.Online;
                curUser = await _vrchat.Users.UpdateUserAsync(curUser.Id, new UpdateUserRequest()
                {
                    Status = UserStatus.JoinMe,
                    StatusDescription = "Come Party At Club Orbital!",
                    Birthday = DateTime.Parse("11/16/1993"),
                });
            }
        }
        public static async Task StopAsync()
        {
            Debug.Log("Stoping VRChat API Client...", header: true);
            await _vrchat.Authentication.LogoutAsync();
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
            if (!_vrchat.IsLoggedIn || ConsoleProgram.state != ConsoleProgram.StateEnum.Running) return;
            //Debug.Log("Checking notification...", header: true);
            var notifications = await _vrchat.Notifications.GetNotificationsAsync();
            if (notifications == null) return;
            foreach (var notification in notifications)
            {
                switch (notification.Type)
                {
                    case NotificationType.FriendRequest: await _vrchat.Notifications.AcceptFriendRequestAsync(notification.Id); return;
                    case NotificationType.RequestInvite:
                        break;
                    case NotificationType.Invite:
                        var instance = await _vrchat.Instances.GetInstanceAsync("wrld_b096eca5-e552-40c7-8db5-7ad74cc8100e", "Events");
                        notification.Validate(new ValidationContext(instance));
                        notification.Seen = true;
                        break;
                    default:
                        Debug.Log(notification.ToJson(), header: true);
                        break;
                }
            }
            //Debug.Log("Notification checking done!", header: true);
            await Task.Delay(5000);
            await CheckNotification();
        }


        public static async Task SendEventInvite(User user)
        {
            try
            {

                // Invite User
                var curUser = await _vrchat.Authentication.GetCurrentUserAsync();
                var inviteRequest = new InviteRequest($"{worldid}:{instanceID}~private({curUser.Id})~canRequestInvite~region(use)~nonce({Guid.NewGuid()})");
                Notification result = await _vrchat.Invites.InviteUserAsync(user.Id, inviteRequest);
                Debug.Log(result, header: true);
            }
            catch (ApiException e)
            {
                Debug.Log("Exception when calling InviteApi.InviteUser: " + e.Message, Color.Red, true);
                Debug.Log("Status Code: " + e.ErrorCode, Color.Red, true);
                Debug.Log(e.StackTrace, Color.Red, true);
            }
        }

        public static async Task SendFriendInvite(User user)
        {
            try
            {
                // Invite User
                var curUser = await _vrchat.Authentication.GetCurrentUserAsync();
                if (!user.IsFriend)
                {
                    Notification result = await _vrchat.Friends.FriendAsync(user.Id);
                    Debug.Log(result, header: true);
                }
            }
            catch (ApiException e)
            {
                Debug.Log("Exception when calling InviteApi.InviteUser: " + e.Message, Color.Red, true);
                Debug.Log("Status Code: " + e.ErrorCode, Color.Red, true);
                Debug.Log(e.StackTrace, Color.Red, true);
            }
        }

        public static async Task<User> GetUserByIdAsync(string userid)
        {
            try
            {
                Debug.Log("Getting Other Player Info", header: true);
                User OtherUser = await _vrchat.Users.GetUserAsync(userid);
                if (OtherUser == null) return default;
                Debug.Log($"Found user {OtherUser.DisplayName}, joined {OtherUser.DateJoined}", header: true);
                return Task.FromResult(OtherUser).Result;
            }
            catch (ApiException e)
            {
                 Debug.Log($"Exception when calling API: {e.Message}", Color.Red);
                 Debug.Log($"Status Code: {e.ErrorCode}", Color.Red);
                 Debug.Log(e.ToString(), Color.Red);
                return default;
            }
        }
        public static async Task<User> GetUserByNameAsync(string username, bool logerror = true)
        {
            try
            {
                Debug.Log("Getting Other Player Info", header: true);
                var OtherUser = await _vrchat.Users.GetUserByNameAsync(username);
                if (OtherUser == null) return default;
                Debug.Log($"Found user {OtherUser.DisplayName}, joined {OtherUser.DateJoined}", header: true);
                return Task.FromResult(OtherUser).Result;
            }
            catch (ApiException e)
            {
                if(logerror)
                {
                    Debug.Log($"Exception when calling API: {e.Message}" , Color.Red);
                    Debug.Log($"Status Code: {e.ErrorCode}", Color.Red);
                    Debug.Log(e.ToString(), Color.Red);
                }
                return default;
            }
        }
        public static async Task<User> SearchAsync(string displayname, bool logerror = true)
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
                if(logerror)
                {
                    Debug.Log($"Exception when calling API: {e.Message}", Color.Red);
                    Debug.Log($"Status Code: {e.ErrorCode}", Color.Red);
                    Debug.Log(e.ToString(), Color.Red);
                }
                return default;
            }
        }
        public static async Task<World> GetWorldByIdAsync(string worldid)
        {
            try
            {
                Debug.Log("Getting World Info", header: true);
                World World = await _vrchat.Worlds.GetWorldAsync(worldid);
                if (World == null) return default;
                Debug.Log($"Found world {World.Name}, visits: {World.Visits}", header: true);
                return Task.FromResult(World).Result;
            }
            catch (ApiException e)
            {
                 Debug.Log($"Exception when calling API: {e.Message}", Color.Red);
                 Debug.Log($"Status Code: {e.ErrorCode}", Color.Red);
                 Debug.Log(e.ToString(), Color.Red);
                return default;
            }
        }
    }
}