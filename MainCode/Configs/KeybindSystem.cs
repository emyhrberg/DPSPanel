using Terraria.ModLoader;
using Terraria;
using DPSPanel.MainCode.Panel;

namespace DPSPanel.MainCode.Configs
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
            // Check if the keybind was pressed
            if (toggleDPSPanelKeybind?.JustPressed == true)
            {
                // Get the PanelSystem instance
                var uiSystem = ModContent.GetInstance<PanelSystem>();

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

                // Toggle the DPS panel
                uiSystem.state.ToggleDPSPanel();
            }
        }
    }
}
