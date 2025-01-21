using Terraria.ModLoader;
using Terraria;
using DPSPanel.Core.Panel;
using Microsoft.Xna.Framework;

namespace DPSPanel.Core.Configs
{
    public class KeybindSystem : ModSystem
    {
        public static ModKeybind toggleDPSPanelKeybind { get; private set; }

        public override void Load()
        {
            // Register the keybind
            toggleDPSPanelKeybind = KeybindLoader.RegisterKeybind(Mod, "ToggleDPSPanel", "K");
        }

        public override void Unload()
        {
            // Null out the keybind to avoid issues during unloading
            toggleDPSPanelKeybind = null;
        }

        public override void PostUpdateInput()
        {
            if (toggleDPSPanelKeybind?.JustPressed == true)
            {
                // Get the PanelSystem instance
                PanelSystem uiSystem = ModContent.GetInstance<PanelSystem>();

                // Ensure PanelSystem and its state exist
                if (uiSystem == null)
                {
                    Mod.Logger.Warn("PanelSystem instance is null. Make sure it is initialized.");
                    return;
                }

                if (uiSystem.state == null)
                {
                    Mod.Logger.Warn("PanelSystem state is null. Cannot toggle DPS panel.");
                    return;
                }

                bool vis = uiSystem.state.isVisible;
                string onOff = vis ? "ON" : "OFF";
                Main.NewText($"Damage Panel: [{onOff}]. Press K to toggle.", Color.Yellow);
                uiSystem.state.ToggleDPSPanel();
            }
        }
    }
}
