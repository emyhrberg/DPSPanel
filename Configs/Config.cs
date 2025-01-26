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
        [BackgroundColor(247, 186, 197)] // Light pink
        [Tooltip("Basically, if you are using weapons it will not highlight the toggle button when hovering over the button.")]
        [DefaultValue(true)]
        public bool DisableValidHoverHighlight { get; set; } = false;

        [Header("EvenMoreSettings")]
        [Label("Popup Message")]
        [BackgroundColor(35, 132, 250)] // CornflowerBlue
        [Tooltip("Shows a popup message every time when entering a new world.")]
        [DefaultValue(true)]
        public bool ShowPopupMessage { get; set; } = false;
    }
}
