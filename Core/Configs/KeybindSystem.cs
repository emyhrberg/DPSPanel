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
                Config c = ModContent.GetInstance<Config>();
                if (!c.EnableButton)
                {
                    Main.NewText($"Damage panel is disabled, enable it in configuration.", Color.Yellow);
                    return;
                }

                PanelSystem uiSystem = ModContent.GetInstance<PanelSystem>();
                bool vis = uiSystem.state.container.panelVisible;
                string onOff = vis ? "ON" : "OFF";
                Main.NewText($"Damage Panel: [{onOff}]. Press K to toggle.", Color.Yellow);
                uiSystem.state.container.TogglePanel();
            }
        }
    }
}
