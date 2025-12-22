using Microsoft.AspNetCore.SignalR;

namespace _20241129612SoruCevapPortalı.Hubs
{
    public class PortalHub : Hub
    {
        public async Task SendNewQuestionNotification(string user, string title)
        {
            await Clients.All.SendAsync("ReceiveNotification", user, title);
        }
    }
}