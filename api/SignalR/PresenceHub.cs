using api.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace api.SignalR
{
    public class PresenceHub : Hub
    {
        [Authorize] // only authorized users are able to access this hub
        public override async Task OnConnectedAsync()
        {
            // Clients object can be used to invoke methods on clients 
            // that are connected to this hub

            // we've got the option to send a message to all connected users,
            // or just to the user that's calling this method on the hub,
            // or send it to everyone else apart from the user that's just connected

            // here when this user does connect
            // anybody else that's connected to this same hub
            // is going to receive the username of the user that has just connected,
            // and Context give us access to our user claims principle
            await Clients.Others.SendAsync("UserIsOnline", Context.User.GetUsername());
        }

        public override async  Task OnDisconnectedAsync(Exception exception)
        {
            // Clients object also can be used to invoke methods on clients 
            // that are disconnected to this hub

            // here when this user does disconnect
            // anybody else that's connected to this same hub
            // is going to receive the username of the user that has just disconnected,
            // and Context give us access to our user claims principle
            await Clients.Others.SendAsync("UserIsOffline", Context.User.GetUsername());

            // because we're passing an exception to this method,
            // we do need to call the base onDisconnected method
            await base.OnDisconnectedAsync(exception);
        }
    }
}