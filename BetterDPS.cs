using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria.UI;
using BetterDPS.Content.UI;

namespace BetterDPS
{
	// Please read https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-Guide#mod-skeleton-contents for more information about the various files in a mod.
	public class BetterDPS : Mod
	{

        public override void Load()
        {
            MenuBar menuBar = new MenuBar();
            menuBar.Activate();
            UserInterface _menuBar = new UserInterface();
            _menuBar.SetState(menuBar);
        }

        public override void Unload()
        {
            base.Unload();
        }

    }
}
