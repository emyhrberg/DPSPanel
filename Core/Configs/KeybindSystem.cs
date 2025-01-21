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
            toggleDPSPanelKeybind = null;
        }

        public override void PostUpdateInput()
        {
            if (toggleDPSPanelKeybind?.JustPressed == true)
            {
                PanelSystem uiSystem = ModContent.GetInstance<PanelSystem>();
                bool vis = uiSystem.state.isVisible;
                string onOff = vis ? "ON" : "OFF";
                Main.NewText($"Damage Panel: [{onOff}]. Press K to toggle.", Color.Yellow);
                uiSystem.state.ToggleDPSPanel();
            }
        }
    }
}
