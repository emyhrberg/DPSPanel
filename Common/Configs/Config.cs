using System.Collections.Generic;
using System.ComponentModel;
using DPSPanel.Helpers;
using DPSPanel.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace DPSPanel.Common.Configs
{
    public class Config : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        [Header("DamageCalculation")]
        [BackgroundColor(192, 54, 64)] // Calamity Red
        [DefaultValue(true)]
        public bool TrackAllEntities;

        // Calamity Red
        [DefaultValue(true)]
        [BackgroundColor(192, 54, 64)] // Calamity Red
        public bool TrackUnknownDamage;

        [Header("UI")]
        [DrawTicks]
        [OptionStrings(["Default", "Fancy", "FancyFTW", "FancyLegendary", "FancyPlat", "Golden", "Leaf", "Remix", "Retro", "Sticks", "StoneGold", "Thin", "Tribute", "TwigLeaf", "Valkyrie",])]
        [DefaultValue("Default")]
        [BackgroundColor(255, 192, 8)] // Golden Yellow
        public string Theme;

        [DrawTicks]
        [OptionStrings(["Small", "Medium", "Large",])]
        [DefaultValue("Small")]
        [BackgroundColor(255, 192, 8)] // Golden Yellow
        public string Width;

        [DrawTicks]
        [OptionStrings(["Small", "Medium", "Large",])]
        [DefaultValue("Small")]
        [BackgroundColor(255, 192, 8)] // Golden Yellow
        public string Height;

        [Header("Settings")]

        [CustomModConfigItem(typeof(ShowPlayerIconConfigElement))]
        [BackgroundColor(69, 80, 232)] // Damp Blue
        [DefaultValue(true)]
        public bool ShowPlayerIcons;

        [BackgroundColor(69, 80, 232)] // Damp Blue
        [DefaultValue(true)]
        public bool ShowBossIcon;

        [BackgroundColor(69, 80, 232)] // Damp Blue
        [DefaultValue(true)]
        public bool ShowTooltipWhenHovering;

        [BackgroundColor(69, 80, 232)] // Damp Blue
        [DefaultValue(true)]
        public bool ShowWeaponsDuringBossFight;

        [BackgroundColor(69, 80, 232)] // Damp Blue
        [DefaultValue(true)]
        public bool MakePanelDraggable;

        public override void OnChanged()
        {
            // null check #1
            Config c = ModContent.GetInstance<Config>();
            if (c == null)
            {
                Log.Info("Config is null in Config.OnChanged()");
                return;
            }

            // null check #2
            MainSystem sys = ModContent.GetInstance<MainSystem>();
            if (sys == null)
            {
                Log.Info("MainSystem is null in Config.OnChanged()");
                return;
            }

            UpdateTheme(sys);
            UpdateWidth(sys);
        }

        private static void UpdateTheme(MainSystem sys)
        {
            // Update the emptyBar and fullBar in WeaponBar.
            Dictionary<string, WeaponBar> weaponBars = sys.state.container.panel.WeaponBars;

            foreach (var kvp in weaponBars)
            {
                string theme = Conf.C.Theme;

                // Get the corresponding asset.
                Asset<Texture2D> emptyBar = typeof(Ass).GetField(theme)?.GetValue(null) as Asset<Texture2D>;
                if (emptyBar == null)
                {
                    Log.Error($"Asset for theme \"{theme}\" not found.");
                }
                else
                {
                    Log.Info($"Applying theme \"{theme}\" to weapon bar \"{kvp.Key}\".");
                    WeaponBar weaponBar = kvp.Value;
                    weaponBar.UpdateTheme(emptyBar);
                }
            }
        }

        private void UpdateWidth(MainSystem sys)
        {
            // This is straightforward.
            // Set the width of the container based on the selected option.
            MainContainer mainContainer = sys.state.container;

            mainContainer.Width.Pixels = SizeHelper.WidthSizes[Conf.C.Width];
            mainContainer.panel.Width.Pixels = SizeHelper.WidthSizes[Conf.C.Width];
            Log.Info("new width pixels: " + mainContainer.Width.Pixels);
            sys.state.container.Recalculate();
            // Also update the bar asset, otherwise it will look stretched out and ugly.
            // Meaning its 300 px version or 400 px version etc.
        }

        public static class Conf
        {
            // C = instance
            // Example usage: Conf.C.ShowPlayerIcons...
            public static Config C => ModContent.GetInstance<Config>();
        }
    }
}
