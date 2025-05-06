using Microsoft.AspNetCore.SignalR;

namespace DomainModels.Models
{
    public class GameHub : Hub
    {
        public enum HubEvents
        {
            GameStart = 1,
            GameProgress = 2,
            GameFinished = 3
        }

        public class SocketPack
        {
            public HubEvents EventType;
            required public string SerializedData;
        }
    }
}
