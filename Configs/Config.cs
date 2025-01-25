using Terraria.ModLoader.Config;
using System.ComponentModel;

namespace DPSPanel.Configs
{
    public class SimpleConfig : ModConfig
    {
        // Determines the scope of the configuration. ClientSide means it only affects the player's local game.
        public override ConfigScope Mode => ConfigScope.ClientSide;

        [Header("BasicSettings")]

        [BackgroundColor(255, 99, 71)]
        [DefaultValue(true)] // Default value is true.
        public bool ShowToggleButton { get; set; } = false;

        [BackgroundColor(255, 99, 71)]
        [DefaultValue(false)] // Default value is true.
        public bool ShowWeaponIcon { get; set; } = false;

        [BackgroundColor(255, 99, 71)]
        [DefaultValue(false)] // Default value is true.
        public bool ShowBossIcon { get; set; } = false;
    }
}
