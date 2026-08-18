using Contract.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Presentation.Hubs
{

    [Authorize(AuthenticationSchemes = "Bearer")] // 👈 BU MÜTLƏQ YAZILMALIDIR!
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            // Debug üçün bunu yazın ki, konsolda görək kim qoşulub
            var id = Context.UserIdentifier;
            Console.WriteLine($"🔴 SIGNALR QOŞULDU. User ID: '{id}'");

            await base.OnConnectedAsync();
        }
    }
}
