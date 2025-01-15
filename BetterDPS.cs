using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria;

namespace BetterDPS
{
    // Please read https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-Guide#mod-skeleton-contents for more information about the various files in a mod.

    /// <summary>
    /// This is the main class for the BetterDPS mod.
    /// </summary>
    public class BetterDPS : Mod
	{

        // Variables
        ModKeybind toggleDPSPanelKeybind;

        public override void Load()
        {
            // Register keybind
            toggleDPSPanelKeybind = KeybindLoader.RegisterKeybind(this, "Toggle DPS Panel", "K");
            
        }


        public override void Unload()
        {
            // Unregister keybind
            toggleDPSPanelKeybind = null;
        }
    }
}
