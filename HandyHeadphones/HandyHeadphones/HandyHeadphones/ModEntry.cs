using HandyHeadphones.API.Interfaces;
using Harmony;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HandyHeadphones
{
    public class ModEntry : Mod
    {
        internal static IMonitor monitor;
        internal static IModHelper modHelper;

        internal static readonly string hatsPath = Path.Combine("assets", "HeadphonesPack");

        public override void Entry(IModHelper helper)
        {
            // Set up the monitor
            monitor = Monitor;
            modHelper = helper;

            // Load our Harmony patches
            try
            {
                var harmony = HarmonyInstance.Create(this.ModManifest.UniqueID);
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            catch (Exception e)
            {
                Monitor.Log($"Issue with Harmony patch: {e}", LogLevel.Error);
                return;
            }

            // Hook into the game launch
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;

            // Hook into the player warping
            helper.Events.Player.Warped += this.OnWarped;
        }

        private void OnWarped(object sender, WarpedEventArgs e)
        {
            Hat playerHat = e.Player.hat;

            if (playerHat is null || playerHat.Name != "Headphones")
            {
                return;
            }

            e.NewLocation.miniJukeboxTrack.Value = e.OldLocation.miniJukeboxTrack.Value;
            e.OldLocation.miniJukeboxTrack.Value = "";
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
