using System.Threading.Tasks;

namespace TruelimeBackend.Helpers
{
    public interface IHubClient
    {
        Task BroadcastMessage();
    }
}