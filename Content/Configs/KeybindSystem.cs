using Terraria.ModLoader;
using BetterDPS.UI.DPS;

namespace BetterDPS.Content.Configs
{
    // Acts as a container for keybinds registered by this mod.
    // See Common/Players/ExampleKeybindPlayer for usage.
    public class KeybindSystem : ModSystem
    {
        public static ModKeybind toggleDPSPanelKeybind { get; private set; }


        public override void Load()
        {
            // Registers a new keybind
            // We localize keybinds by adding a Mods.{ModName}.Keybind.{KeybindName} entry to our localization files. The actual text displayed to English users is in en-US.hjson
            toggleDPSPanelKeybind = KeybindLoader.RegisterKeybind(Mod, "ToggleDPSPanel", "K");
        }

        // Please see ExampleMod.cs' Unload() method for a detailed explanation of the unloading process.
        public override void Unload()
        {
            // Not required if your AssemblyLoadContext is unloading properly, but nulling out static fields can help you figure out what's keeping it loaded.
            toggleDPSPanelKeybind = null;
        }


        public override void PostUpdateInput()
        {
            if (toggleDPSPanelKeybind.JustPressed)
            {
                // Toggle the DPS panel
                var container = ModContent.GetInstance<DPSPanelSystem>().container;
                container.ToggleDPSPanel();
            }

        }
    }
}