using System.ComponentModel;
using DPSPanel.UI;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace DPSPanel.Core.Configs
{
    public class Config : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        [Header("DamageCalculation")]
        // boss often spawns minions so we need to check if we track damage to only the boss or to all entities during the fight
        [BackgroundColor(192, 54, 64)] // Calamity Red
        [DefaultValue(true)]
        public bool TrackAllEntities { get; set; } = true;

        [Header("PanelSettings")]
        [BackgroundColor(192, 54, 64)] // Calamity Red
        [DefaultValue(true)]
        public bool ShowPlayerIcons { get; set; } = true;

        [BackgroundColor(192, 54, 64)] // Calamity Red
        [DefaultValue(true)]
        public bool ShowBossIcon { get; set; } = true;

        [BackgroundColor(192, 54, 64)] // Calamity Red
        [DefaultValue(true)]
        public bool ShowOnlyWhenInventoryOpen { get; set; } = true;

        [BackgroundColor(192, 54, 64)] // Calamity Red
        [DefaultValue(true)]
        public bool HighlightButtonWhenHovering { get; set; } = true;

        [BackgroundColor(192, 54, 64)] // Calamity Red
        [DefaultValue(10)]
        [Range(1, 10)]
        public int MaxWeaponsDisplayed { get; set; } = 10;

        [Range(150, 300)]
        [Increment(150)]
        [DrawTicks]
        [DefaultValue(150)]
        [BackgroundColor(192, 54, 64)] // Calamity Red
        public int BarWidth { get; set; } = 300;

        public override void OnChanged()
        {
            Config c = ModContent.GetInstance<Config>();
            if (c == null) return;

            // update stuff after config changes
            UpdateBarWidth(c);
        }

        private static void UpdateBarWidth(Config c)
        {
            MainContainer.UpdateBarWidth(c);
            PlayerBar.UpdateBarWidth(c);
            Panel.UpdateBarWidth(c);
            WeaponBar.UpdateBarWidth(c);
        }
    }
}
