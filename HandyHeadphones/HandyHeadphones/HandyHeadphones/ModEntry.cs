using HandyHeadphones.API.Interfaces;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandyHeadphones
{
    public class ModEntry : Mod
    {
        internal static IMonitor monitor;
        internal static readonly string hatsPath = Path.Combine("assets", "HeadphonesPack");

        public override void Entry(IModHelper helper)
        {
            // Set up the monitor
            monitor = Monitor;

            // Hook into the game launch
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            // Check if spacechase0's JsonAssets is in the current mod list
            if (Helper.ModRegistry.IsLoaded("spacechase0.JsonAssets"))
            {
                Monitor.Log("Attempting to hook into spacechase0.JsonAssets.", LogLevel.Debug);
                ApiManager.HookIntoJsonAssets(Helper);

                // Add the headphones asset
                ApiManager.GetJsonAssetInterface().LoadAssets(Path.Combine(Helper.DirectoryPath, hatsPath));
            }
        }
    }
}
