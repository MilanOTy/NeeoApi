using Home.Neeo.Interfaces;
using Home.Neeo.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Home.Neeo.Recipe
{
    public class Factory
    {
        private class PowerState
        {
            [JsonProperty("active")]
            public bool Active { get; set; }
        }
        static Func<Task<T>> BuildFunction<T>(string url, IRestClient restClient)
        {
            return async () =>
            {
                NEEOEnvironment.Logger.LogDebug($"Factory | Function {url}");
                return await restClient.HttpGet<T>(url);
            };
        }
        static Func<Task<bool>> BuildGetPowerStateFunction(string url, IRestClient restClient)
        {
            return async () => {
                NEEOEnvironment.Logger.LogDebug($"Factory | GET powerstate {url}");
                var powerState = await restClient.HttpGet<PowerState>(url);
                return powerState != null && powerState.Active;
            };
        }

        internal static void BuildRecipesModel(IEnumerable<NEEORecipe> recipes, IRestClient restClient)
        {
            foreach (var recipe in recipes)
            {
                if (recipe.Url != null)
                {
                    recipe.Action = new NEEORecipe.ActionData
                    {
                        Identify = BuildFunction<bool>(recipe.Url.Identify, restClient),
                        PowerOn = BuildFunction<bool>(recipe.Url.SetPowerOn, restClient),
                        PowerOff = BuildFunction<bool>(recipe.Url.SetPowerOff, restClient),
                        GetPowerState = BuildGetPowerStateFunction(recipe.Url.GetPowerState, restClient)
                    };
                }
            }
        }
    }
}

