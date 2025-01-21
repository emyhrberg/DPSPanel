//using Terraria.ModLoader;
//using Terraria.ModLoader.Config;
//using Microsoft.Xna.Framework;

//namespace DPSPanel.Core.Configs
//{
//    public class Config : ModConfig
//    {
//        // Determines the scope of the configuration. ClientSide means it only affects the player's local game.
//        public override ConfigScope Mode => ConfigScope.ClientSide;

//        // Config category
//        [Label("Common Settings")]
//        public CommonConfig Common { get; set; } = new();

//        [Label("Boss Log Customization")]
//        public BossLogConfig BossLogConfiguration { get; set; } = new();
//    }

//    public class CommonConfig
//    {
//        [Label("Invalid Change Message")]
//        [Tooltip("Displayed when an invalid change attempt is made.")]
//        public string InvalidChange { get; set; } = "You cannot change the '{0}' config while {1} is active!";

//        [Label("Host Change Message")]
//        [Tooltip("Message displayed when only the host can make changes.")]
//        public string HostChange { get; set; } = "Only the host is allowed to change this config.";
//    }

//    public class BossLogConfig
//    {
//        [Header("Boss Log UI")]

//        [Label("Boss Log Color")]
//        [Tooltip("Choose the color of your Boss Log!")]
//        public Color BossLogColor { get; set; } = new Color(255, 235, 110);

//        [Label("Open Log Button Position")]
//        [Tooltip(
//            "Alternatively, hold right-click while hovering over the Boss Log button to move it wherever you'd like with ease!\n" +
//            "Position is measured from the bottom right corner of the screen.")]
//        public Vector2 BossLogPos { get; set; } = new Vector2(100, 100);

//        [Label("Enable Interactions Tab")]
//        [Tooltip(
//            "When enabled, a small tab can be hovered over for an explanation on how to use the alternative interactions of the current page.\n" +
//            "Examples include right-clicking on a Table of Contents entry to manually mark it as defeated.")]
//        public bool ShowInteractionTooltips { get; set; } = true;

//        [Label("[i:3625] [c/ffeb6e:Debug Tools for Mod Developers] [i:3619]")]
//        [Tooltip("Enable debugging features for mod developers.")]
//        public bool Debug { get; set; } = false;
//    }
//}
