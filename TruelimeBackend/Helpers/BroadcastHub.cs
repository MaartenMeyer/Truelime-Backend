using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace TruelimeBackend.Helpers
{
    [AllowAnonymous]
    public class BroadcastHub : Hub
    {
        
    }
}