using Home.Neeo.Device.Brain;
using Home.Neeo.Interfaces;
using Home.Neeo.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Home.Neeo.Recipe
{
    internal class RecipeModule
    {
        private static IRestClient  _restClient;
        private static ILogger      _logger;

        static RecipeModule ()
        {
            _restClient = NEEOEnvironment.RestClient;
            _logger = NEEOEnvironment.Logger;
        }
        internal static async Task<NEEORecipe[]> GetAllRecipes (NEEOBrain brain)
        {
            if (brain == null)
            {
                throw new NEEOException("MISSING_BRAIN_PARAMETER");
            }
            string url = UrlBuilder.BuildBrainUrl(brain, NEEOUrls.BASE_URL_GETRECIPES);
            _logger.LogDebug($"Recipe | GET AllRecipes {url}");
            NEEORecipe[] recipes = await _restClient.HttpGet<NEEORecipe[]>(url);
            if (recipes != null)
                Factory.BuildRecipesModel(recipes, _restClient);
            return recipes;
        }

        internal static async Task<NEEORecipe[]> GetRecipesPowerState (NEEOBrain brain)
        {
            if (brain == null)
            {
                throw new NEEOException("MISSING_BRAIN_PARAMETER");
            }
            string url = UrlBuilder.BuildBrainUrl(brain, NEEOUrls.BASE_URL_GETACTIVERECIPES);
            _logger.LogDebug($"Recipe | GET RecipesPowerState {url}");
            NEEORecipe[] recipes = await _restClient.HttpGet<NEEORecipe[]>(url);
            if (recipes != null)
                Factory.BuildRecipesModel(recipes, _restClient);
            return recipes;
        }
    }
}
