using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Microsoft.Xna.Framework;
using System.ComponentModel;

namespace DPSPanel.Core.Configs
{
    public class Config : ModConfig
    {
        // Determines the scope of the configuration. ClientSide means it only affects the player's local game.
        public override ConfigScope Mode => ConfigScope.ClientSide;

        // basic settings
        [Label("[i:560] [c/ffeb6e:Show Button]")]
        [Tooltip("Shows the button. You can also toggle with hotkey (K) or command (/dps toggle)")]
        [BackgroundColor(255, 99, 71)]
        [DefaultValue(true)] // Default value is true.
        public bool EnableButton { get; set; } = false;

        //[Label("Show Boss Name And Title")]
        //[BackgroundColor(255, 99, 71)]
        //[DefaultValue(true)] // Default value is true.
        //[Tooltip("Shows the boss name and icon.")]
        //public bool ShowBossName { get; set; } = false;

        [Label("Theme")]
        [Tooltip("Changes the theme of the boss damage bar.")]
        [BackgroundColor(255, 99, 71)]
        [DrawTicks]
        [OptionStrings(["Fancy", "Generic"])]
        [DefaultValue("Generic")]
        public string Theme { get; set; }

        [Label("Position")]
        [Tooltip("Changes the position of the icon. You can also hold right click and drag the icon.")]
        [BackgroundColor(250, 235, 215)]
        [SliderColor(87, 181, 92)]
        [DefaultValue(typeof(Vector2), "-270, -50")]
        [Range(-1920f, 0f)]
        public Vector2 BossLogPos { get; set; }

        //Todo uncmment this and start implementing config.

        // Config category
        //[Header("UI")]
        //public CommonConfig UI { get; set; } = new();

        //[Header("BossLogCustomization")]
        //public BossLogConfig BossLogConfiguration { get; set; } = new();
    }
}
