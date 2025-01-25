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

        [Label("[i:271] Show Player Icon")]
        [Tooltip("Set to true to show player icons.")]
        [DefaultValue(true)]
        public bool ShowPlayerIcon { get; set; } = true;

        [Header("AdvancedSettings")]
        [BackgroundColor(255, 182, 193)] // LightPink

        [Label("[i:560] [c/9bfff0:Do nothing for now]")]
        [Tooltip("IDK")]
        [DefaultValue(true)]
        public bool DoNothing { get; set; } = true;

        [Label("[g:25][g:0][g:1][g:2][g:3][g:4][g:5] [c/000000:Do nothing for now]")]
        [Tooltip("IDK")]
        [DefaultValue(true)]
        public bool DoNothing2 { get; set; } = true;

        [Label("[g:25][g:0][g:1][g:2][g:3][g:4][g:5] [c/000000:Do nothing for now]")]
        [Tooltip("IDK")]
        [DefaultValue(true)]
        public bool DoNothing3 { get; set; } = true;

        [Label("[a:TIMBER] [c/000000:Timber!!]")]
        [Tooltip("Achievement: Chop down your first tree.")]
        [DefaultValue(true)]
        public bool DoNothing4 { get; set; } = true;

        [Label("[a:NO_HOBO] [c/000000:No Hobo]")]
        [Tooltip("Achievement: Build a house suitable enough for your first town NPC.")]
        [DefaultValue(true)]
        public bool DoNothing5 { get; set; } = true;

        [Label("[a:OBTAIN_HAMMER] [c/000000:Stop! Hammer Time!]")]
        [Tooltip("Achievement: Obtain your first hammer.")]
        [DefaultValue(true)]
        public bool DoNothing6 { get; set; } = true;

        [Label("[a:HEART_BREAKER] [c/000000:Heart Breaker]")]
        [Tooltip("Achievement: Smash a heart crystal underground.")]
        [DefaultValue(true)]
        public bool DoNothing7 { get; set; } = true;

        [Label("[a:OOO_SHINY] [c/000000:Ooo! Shiny!]")]
        [Tooltip("Achievement: Mine your first nugget of ore with a pickaxe.")]
        [DefaultValue(true)]
        public bool DoNothing8 { get; set; } = true;

        [Label("[a:HEAVY_METAL] [c/000000:Heavy Metal]")]
        [Tooltip("Achievement: Obtain an anvil made from iron or lead.")]
        [DefaultValue(true)]
        public bool DoNothing9 { get; set; } = true;

        [Label("[a:I_AM_LOOT] [c/000000:I Am Loot!]")]
        [Tooltip("Achievement: Discover a golden chest underground and take a peek inside.")]
        [DefaultValue(true)]
        public bool DoNothing10 { get; set; } = true;

        [Label("[a:STAR_POWER] [c/000000:Star Power]")]
        [Tooltip("Achievement: Craft a mana crystal out of fallen stars and consume it.")]
        [DefaultValue(true)]
        public bool DoNothing11 { get; set; } = true;

        [Label("[a:HOLD_ON_TIGHT] [c/000000:Hold on Tight!]")]
        [Tooltip("Achievement: Equip your first grappling hook.")]
        [DefaultValue(true)]
        public bool DoNothing12 { get; set; } = true;

        [Label("[a:EYE_ON_YOU] [c/000000:Eye on You]")]
        [Tooltip("Achievement: Defeat the Eye of Cthulhu.")]
        [DefaultValue(true)]
        public bool DoNothing13 { get; set; } = true;
    }
}
