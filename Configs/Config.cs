using Terraria.ModLoader.Config;
using System.ComponentModel;

namespace DPSPanel.Configs
{
    public class Config : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        [Header("BasicSettings")]
        [BackgroundColor(100, 149, 237)] // CornflowerBlue. Is used for all items in BasicSettings?

        [Label("[i:256] Always Show Button")]
        [Tooltip("On: Always show the ninja button. Off: Only show the button when inventory is open.")]
        [DefaultValue(true)]
        public bool AlwaysShowButton { get; set; } = true;

        [Header("AdvancedSettings")]
        [BackgroundColor(255, 182, 193)] // LightPink

        [Label("[i:560] [c/9bfff0:Do nothing for now]")]
        [Tooltip("IDK")]
        [DefaultValue(true)]
        public bool DoNothing { get; set; } = true;
    }
}
