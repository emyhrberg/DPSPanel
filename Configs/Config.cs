using Terraria.ModLoader.Config;
using System.ComponentModel;

namespace DPSPanel.Configs
{
    public class Config : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        [Header("BasicSettings")]
        // [BackgroundColor(100, 149, 237)] // CornflowerBlue

        [Label("[i:256] Always Show Button")]
        [BackgroundColor(35, 132, 250)] // CornflowerBlue
        [Tooltip("On: Always show the ninja button. Off: Only show the button when inventory is open.")]
        [DefaultValue(true)]
        public bool AlwaysShowButton { get; set; } = true;

        [Label("[i:271] Show Player Icon")]
        [BackgroundColor(35, 132, 250)] // CornflowerBlue
        [Tooltip("Set to true to show player icons.")]
        [DefaultValue(true)]
        public bool ShowPlayerIcon { get; set; } = true;

        [Header("AdvancedSettings")]

        [Label("Disable Valid Hover Highlight")]
        [BackgroundColor(35, 132, 250)] // CornflowerBlue
        [Tooltip("If enabled, disables the valid hover highlight during certain mouse interactions.")]
        [DefaultValue(false)]
        public bool DisableValidHoverHighlight { get; set; } = false;

        [Label("Show Clear Button (currently disabled)")]
        [BackgroundColor(35, 132, 250)] // CornflowerBlue
        [Tooltip("If enabled, shows the clear button.")]
        [DefaultValue(false)]
        public bool ShowClearButton { get; set; } = false;
    }
}
