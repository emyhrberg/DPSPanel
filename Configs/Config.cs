using Terraria.ModLoader.Config;
using System.ComponentModel;
using Terraria.ModLoader;
using DPSPanel.UI;

namespace DPSPanel.Configs
{
    public class Config : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;


        [Header("DamageCalculation")]
        // boss often spawns minions so we need to check if we track damage to only the boss or to all entities during the fight
        [Label("Track Damage to All Entities During Boss Fight")]
        [BackgroundColor(192, 54, 64)] // Calamity Red
        [Tooltip("Enable this to ensure that the boss's minions are also included in the damage calculation.")]
        [DefaultValue(true)]
        public bool TrackAllEntities { get; set; } = true;

        [Header("UI")]

        [Label("Show Toggle Button")]
        [BackgroundColor(192, 54, 64)] // Calamity Red
        [Tooltip("On: Always show the toggle button. Off: Only show the toggle button when you have weapons in your inventory.")]
        [DefaultValue(true)]
        public bool AlwaysShowButton { get; set; } = true;

        [Label("Show Player Icon")]
        [BackgroundColor(192, 54, 64)] // Calamity Red
        [Tooltip("On: Always show player icons.")]
        [DefaultValue(true)]
        public bool ShowPlayerIcon { get; set; } = true;

        [Label("[i:271] Show Boss Icon")]
        [BackgroundColor(192, 54, 64)] // Calamity Red
        [Tooltip("On: Always show boss icons.")]
        [DefaultValue(true)]
        public bool ShowBossIcon { get; set; } = true;

        [Label("Max Weapons Displayed")]
        [BackgroundColor(192, 54, 64)] // Calamity Red
        [Tooltip("Set the maximum number of weapons to display in the panel. (experimental)")]
        [DefaultValue(10)]
        public int MaxWeaponsDisplayed { get; set; } = 10;

        [Label("Highlight Toggle Button When Using Weapons")]
        [BackgroundColor(192, 54, 64)] // Calamity Red
        [Tooltip("Set to false to remove the annoyance of the toggle button flashing when you're using weapons.")]
        [DefaultValue(true)]
        public bool DisableValidHoverHighlight { get; set; } = false;

        [Label("Bar Width")]
        [BackgroundColor(192, 54, 64)] // Calamity Red
        [Tooltip("Set the width of the bars. (experimental)")]
        [DrawTicks]
        [OptionStrings(["150", "300"])]
        [DefaultValue("300")]
        // [ReloadRequired]
        public string BarWidth { get; set; } = "300";

        public override void OnChanged()
        {
            Config c = ModContent.GetInstance<Config>();
            if (c != null)
                UpdateBarWidth(c);
        }

        private void UpdateBarWidth(Config c)
        {
            BossContainerElement.UpdateBarWidth(c);
            DamageBarElement.UpdateBarWidth(c);
            Panel.UpdateBarWidth(c);
            WeaponDamageBarElement.UpdateBarWidth(c);
        }
    }
}
