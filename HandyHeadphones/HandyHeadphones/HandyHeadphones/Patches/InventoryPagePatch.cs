using Harmony;
using StardewValley;
using System.Reflection;
using StardewModdingAPI;
using Microsoft.Xna.Framework;
using StardewValley.Menus;
using System.Linq;
using StardewValley.Objects;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using HandyHeadphones.UI;

namespace HandyHeadphones.Patches
{
    [HarmonyPatch]
    class InventoryPagePatch
    {
        private static IMonitor monitor = ModEntry.monitor;
		private static IModHelper helper = ModEntry.modHelper;

        internal static MethodInfo TargetMethod()
        {
            return AccessTools.Method(typeof(StardewValley.Menus.InventoryPage), nameof(StardewValley.Menus.InventoryPage.receiveLeftClick));
        }

        internal static bool Prefix(InventoryPage __instance, int x, int y, bool playSound = true)
        {
			ClickableComponent hatComponent = __instance.equipmentIcons.First(s => s.name == "Hat");
			if (hatComponent.containsPoint(x, y))
			{
				if (IsHeadphoneHeld())
				{
					return true;
				}

				bool heldItemWasNull = Game1.player.CursorSlotItem is null;
				if (Game1.player.CursorSlotItem.Name == "Headphones" || Game1.player.CursorSlotItem.Name == "Earbuds")
				{
					Hat tmp = (Hat)helper.Reflection.GetMethod(__instance, "takeHeldItem").Invoke<Item>();
					Item heldItem = Game1.player.hat;
					heldItem = Utility.PerformSpecialItemGrabReplacement(heldItem);
					helper.Reflection.GetMethod(__instance, "setHeldItem").Invoke(heldItem);
					Game1.player.hat.Value = tmp;

					if (Game1.player.hat.Value != null)
					{
						Game1.playSound("grassyStep");
					}
					else if (Game1.player.CursorSlotItem is null)
					{
						Game1.playSound("dwop");
					}

					ShowMusicMenu();
				}

				if (!heldItemWasNull || Game1.player.CursorSlotItem is null || !Game1.oldKBState.IsKeyDown(Keys.LeftShift))
				{
					return false;
				}

				for (int l = 0; l < Game1.player.items.Count; l++)
				{
					if (Game1.player.items[l] == null || Game1.player.CursorSlotItem != null && Game1.player.items[l].canStackWith(Game1.player.CursorSlotItem))
					{
						if (Game1.player.CurrentToolIndex == l && Game1.player.CursorSlotItem != null)
						{
							Game1.player.CursorSlotItem.actionWhenBeingHeld(Game1.player);
						}
						helper.Reflection.GetMethod(__instance, "setHeldItem").Invoke(Utility.addItemToInventory(helper.Reflection.GetMethod(__instance, "takeHeldItem").Invoke<Item>(), l, __instance.inventory.actualInventory));
						if (Game1.player.CurrentToolIndex == l && Game1.player.CursorSlotItem != null)
						{
							Game1.player.CursorSlotItem.actionWhenStopBeingHeld(Game1.player);
						}
						Game1.playSound("stoneStep");
						return false;
					}
				}
			}

			if (IsSelectingHeadPhonesInInventory(__instance, x, y) && Game1.oldKBState.IsKeyDown(Keys.LeftShift))
            {
				helper.Reflection.GetMethod(__instance, "setHeldItem").Invoke(__instance.inventory.leftClick(x, y, helper.Reflection.GetMethod(__instance, "takeHeldItem").Invoke<Item>(), !Game1.oldKBState.IsKeyDown(Keys.LeftShift)));
				if (Game1.player.CursorSlotItem != null && Game1.oldKBState.IsKeyDown(Keys.LeftShift))
				{
					if (Game1.player.hat.Value == null)
					{
						Game1.player.hat.Value = helper.Reflection.GetMethod(__instance, "takeHeldItem").Invoke<Item>() as Hat;
						ShowMusicMenu();
						return false;
					}
				}
			}

			return true;
		}

		private static void ShowMusicMenu()
        {
			List<string> list = Game1.player.songsHeard.Distinct().ToList();
			list.Insert(0, "turn_off");
			list.Insert(1, "random");
			Game1.activeClickableMenu = new MusicMenu(list, OnSongChosen, isJukebox: true, Game1.player.currentLocation.miniJukeboxTrack.Value);
		}

		private static void OnSongChosen(string selection)
		{
			if (Game1.player.currentLocation == null)
			{
				return;
			}
			if (selection == "turn_off")
			{
				Game1.player.currentLocation.miniJukeboxTrack.Value = "";
				return;
			}
			if (selection == "random")
			{
				Game1.player.currentLocation.SelectRandomMiniJukeboxTrack();
			}
			Game1.player.currentLocation.miniJukeboxTrack.Value = selection;
		}

		private static bool IsHeadphoneHeld()
        {
			return Game1.player.CursorSlotItem is null || (Game1.player.CursorSlotItem.Name != "Headphones" && Game1.player.CursorSlotItem.Name != "Earbuds");
		}

		private static bool IsSelectingHeadPhonesInInventory(InventoryPage page, int x, int y)
        {
			return page.inventory.getItemAt(x, y) != null && (page.inventory.getItemAt(x, y).Name == "Headphones" || page.inventory.getItemAt(x, y).Name == "Earbuds");
        }
    }
}
