using FileUpload.Models;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FileUpload.Hubs
{

    public class BroadCastHub : Hub
    {
        public static IList<string> connectionIds = new List<string>();

        public Task FeedbackMessage(List<RecordItem> data)
        {
          return  Clients.All.SendAsync("feedBack", data);
        }

        public override Task OnConnectedAsync()
        {
            connectionIds.Add(Context.ConnectionId);
            return base.OnConnectedAsync();
        }
    }

}
