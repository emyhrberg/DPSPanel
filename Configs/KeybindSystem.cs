using Microsoft.Xna.Framework;
using Terraria;
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
                ModContent.GetInstance<Config>().AlwaysShowButton = !ModContent.GetInstance<Config>().AlwaysShowButton;

                Rectangle pos = Main.LocalPlayer.getRect();
                // change color and text based on the config setting
                Color color = ModContent.GetInstance<Config>().AlwaysShowButton ? Color.Green : Color.Red;
                string text = ModContent.GetInstance<Config>().AlwaysShowButton ? "Button will always show" : "Button will only show when inventory is open";
                CombatText.NewText(pos, color, text);
            }
        }
    }
}
