using Terraria.ModLoader.Config;
using System.ComponentModel;

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
        [Tooltip("Set the maximum number of weapons to display in the panel.")]
        [DefaultValue(10)]
        public int MaxWeaponsDisplayed { get; set; } = 10;

        [Label("Highlight Toggle Button When Hovering")]
        [BackgroundColor(192, 54, 64)] // Calamity Red
        [Tooltip("Basically, if you are using weapons it will not highlight the toggle button when hovering over the button.")]
        [DefaultValue(true)]
        public bool DisableValidHoverHighlight { get; set; } = false;


    }
}
