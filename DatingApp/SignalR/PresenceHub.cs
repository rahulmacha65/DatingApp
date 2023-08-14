using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace DatingApp.SignalR
{
    public class PresenceHub:Hub
    {
        private readonly PresenceTracker _tracker;
        public PresenceHub(PresenceTracker tracker)
        {
            _tracker = tracker;
        }
        public override async Task OnConnectedAsync()
        {
            string userName = Context.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            
            await _tracker.UserConnected(userName, Context.ConnectionId);

            await Clients.Others.SendAsync("UserIsOnline",userName);

            var currentUsers = await _tracker.GetOnlineUser();

            await Clients.All.SendAsync("GetOnlineUsers", currentUsers);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            string userName = Context.User.FindFirst(ClaimTypes.NameIdentifier).Value;

            await _tracker.UserDisconnected(userName, Context.ConnectionId);

            await Clients.Others.SendAsync("UserIsOffline", userName);

            var currentUsers = await _tracker.GetOnlineUser();

            await Clients.All.SendAsync("GetOnlineUsers", currentUsers);

            await base.OnDisconnectedAsync(exception);
        }
    }
}
