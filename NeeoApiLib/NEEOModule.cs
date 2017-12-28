using Home.Neeo.Device;
using Home.Neeo.Device.ImplementationService;
using Home.Neeo.Discover;
using Home.Neeo.Interfaces;
using Home.Neeo.Models;
using Home.Neeo.Recipe;
using System.Threading.Tasks;

namespace Home.Neeo
{
    public class NEEOModule
    {
        public static Task<NEEOBrain> DiscoverOneBrain()
        {
            return DiscoverModule.Instance.DiscoverOneBrain();
        }
        public static Task<NEEORecipe[]> GetAllRecipes(NEEOBrain configuration)
        {
            return RecipeModule.GetAllRecipes(configuration);
        }
        public static Task<NEEORecipe[]> GetRecipesPowerState(NEEOBrain configuration)
        {
            return RecipeModule.GetRecipesPowerState(configuration);
        }
        public static DeviceBuilder BuildDevice(string adapterName)
        {
            return DeviceModule.BuildCustomDevice(adapterName, string.Empty);
        }
        public static DeviceState<T> BuildDeviceState<T>(int cacheTimeMs) where T : class
        {
            return DeviceState<T>.BuildInstance(cacheTimeMs);
        }
        public static Task<bool> StartServer (NEEOConf configuration)
        {
            return DeviceModule.StartServer(configuration);
        }
        public static IRequestHandler GetRequestHandler()
        {
            return DeviceModule.GetRequestHandler();
        }
    }
}
