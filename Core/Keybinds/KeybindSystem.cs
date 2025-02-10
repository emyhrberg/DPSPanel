using DPSPanel.Core.Configs;
using DPSPanel.Helpers;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace DPSPanel.Core.Keybinds
{
    public class TogglePanelKeybindSystem : ModSystem
    {
        public static ModKeybind toggleDPSPanelKeybind { get; private set; }

        public override void Load()
        {
            // Register the keybind
            toggleDPSPanelKeybind = KeybindLoader.RegisterKeybind(Mod, "ToggleDPSPanel", "K");
        }

        public override void Unload()
        {
            toggleDPSPanelKeybind = null;
        }

        public override void PostUpdateInput()
        {
            if (toggleDPSPanelKeybind?.JustPressed == true)
            {
                // toggle the button
                ModContent.GetInstance<Config>().ShowOnlyWhenInventoryOpen = !ModContent.GetInstance<Config>().ShowOnlyWhenInventoryOpen;

                // change color and text based on the config setting
                string text = ModContent.GetInstance<Config>().ShowOnlyWhenInventoryOpen ? "Always show DPSPanel" : "Only show when inventory is open";
                Main.NewText(text, Color.White);
            }
        }
    }
}
