using Home.Neeo.Models;
using System.Threading.Tasks;

namespace Home.Neeo.Interfaces
{
    public interface IDiscover
    {
        Task<NEEOBrain> DiscoverOneBrain();
    }
}
