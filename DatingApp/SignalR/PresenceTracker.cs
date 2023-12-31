﻿namespace DatingApp.SignalR
{
    public class PresenceTracker
    {
        private static readonly Dictionary<string,List<string>>
           OnlineUsers = new Dictionary<string, List<string>>();

        public Task UserConnected(string userName,string connectionId)
        {
            lock(OnlineUsers)
            {
                if (OnlineUsers.ContainsKey(userName))
                {
                    OnlineUsers[userName].Add(connectionId);
                }
                else
                {
                    OnlineUsers.Add(userName, new List<string> { connectionId});
                }
            }
            return Task.CompletedTask;
        }

        public Task UserDisconnected(string userName,string connectionId) 
        { 
            lock(OnlineUsers)
            {
                if(!OnlineUsers.ContainsKey(userName)) return Task.CompletedTask;

                OnlineUsers[(userName)].Remove(connectionId);

                if (OnlineUsers[userName].Count == 0)
                {
                    OnlineUsers.Remove(userName);
                }
            }
            return Task.CompletedTask;  
        }

        public Task<string[]> GetOnlineUser()
        {
            string[] onlineUsers;
            lock (OnlineUsers)
            {
                onlineUsers = OnlineUsers.OrderBy(k=>k.Key).Select(k =>k.Key).ToArray();
            }
            return Task.FromResult(onlineUsers);
        }

        public static Task<List<string>> GetConnectionForUser(string userName)
        {
            List<string> connectionIds;

            lock (OnlineUsers)
            {
                connectionIds = OnlineUsers.GetValueOrDefault(userName);
            }

            return Task.FromResult(connectionIds);
        }
    }
}
