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
        public bool ShowHighlightButtonWhenHovering { get; set; } = true;

        [BackgroundColor(192, 54, 64)] // Calamity Red
        [DefaultValue(true)]
        public bool ShowWeaponsDuringBossFight { get; set; } = true;

        public override void OnChanged()
        {
            Config c = ModContent.GetInstance<Config>();
            if (c == null) return;

            // update stuff after config changes
        }
    }
}
