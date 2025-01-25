using Terraria.ModLoader;

namespace DPSPanel.Configs
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
            toggleDPSPanelKeybind = null;
        }

        public override void PostUpdateInput()
        {
            if (toggleDPSPanelKeybind?.JustPressed == true)
            {
                // toggle the button
                ModContent.GetInstance<SimpleConfig>().ShowToggleButton = !ModContent.GetInstance<SimpleConfig>().ShowToggleButton;
            }
        }
    }
}
