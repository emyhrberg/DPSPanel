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

        [Label("[i:560] [c/9bfff0:Do nothing for now]")]
        [BackgroundColor(255, 182, 193)] // LightPink
        [Tooltip("IDK")]
        [DefaultValue(true)]
        public bool DoNothing1 { get; set; } = true;

        [Label("[g:25][g:0][g:1][g:2][g:3][g:4][g:5] [c/000000:Do nothing for now]")]
        [BackgroundColor(255, 182, 193)] // LightPink
        [Tooltip("IDK")]
        [DefaultValue(true)]
        public bool DoNothing2 { get; set; } = true;
    }
}
