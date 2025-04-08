using System;
using System.Collections.Generic;
using System.ComponentModel;
using DPSPanel.Helpers;
using DPSPanel.UI;
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

        [BackgroundColor(192, 54, 64)] // Calamity Red
        [DefaultValue(true)]
        public bool TrackUnknownDamage;

        [Header("UI")]
        [DrawTicks]
        [OptionStrings(["Default", "Fancy", "Golden", "Leaf", "Retro", "Sticks", "StoneGold", "Tribute", "TwigLeaf", "Valkyrie"])]
        [DefaultValue("Default")]
        [BackgroundColor(255, 192, 8)] // Golden Yellow
        public string Theme;

        [DrawTicks]
        [OptionStrings(["Default", "Rainbow",])]
        [DefaultValue("Default")]
        [BackgroundColor(255, 192, 8)] // Golden Yellow
        public string BarColors;

        [DrawTicks]
        [OptionStrings(["Small", "Medium", "Large",])]
        [DefaultValue("Medium")]
        [BackgroundColor(255, 192, 8)] // Golden Yellow
        public string PanelWidth;

        [DrawTicks]
        [OptionStrings(["Small", "Medium", "Large",])]
        [DefaultValue("Medium")]
        [BackgroundColor(255, 192, 8)] // Golden Yellow
        public string BarHeight;

        [Header("Settings")]

        [CustomModConfigItem(typeof(ShowPlayerIconConfigElement))]
        [BackgroundColor(85, 111, 64)] // Damp Green
        [DefaultValue(true)]
        public bool ShowPlayerIcons;

        [BackgroundColor(85, 111, 64)] // Damp Green
        [DefaultValue(true)]
        public bool ShowBossIcon;

        [BackgroundColor(85, 111, 64)] // Damp Green
        [DefaultValue(true)]
        public bool ShowTooltipWhenHovering;

        [BackgroundColor(85, 111, 64)] // Damp Green
        [DefaultValue(true)]
        public bool ShowWeaponsDuringBossFight;

        [BackgroundColor(85, 111, 64)] // Damp Green
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
            UpdateHeight(sys);
        }

        private static void UpdateTheme(MainSystem sys)
        {
            // Update the emptyBar and fullBar in WeaponBar.
            Dictionary<string, WeaponBar> weaponBars = sys.state.container.panel.WeaponBars;

            foreach (var kvp in weaponBars)
            {
                string theme = Conf.C.Theme;

                // Get the corresponding asset.
                Asset<Texture2D> emptyBar = null;
                Asset<Texture2D> fullBar = null;
                if (Conf.C.PanelWidth == "Large")
                {
                    emptyBar = typeof(Ass).GetField($"{theme}Large")?.GetValue(null) as Asset<Texture2D>;
                    fullBar = Ass.BarFillLarge;
                }

                // Custom case for Default and PanelWidth Large, we get the DefaultLarge.
                if (emptyBar != null && fullBar != null)
                {
                    Log.Info($"Applying theme \"{theme}\" to weapon bar \"{kvp.Key}\".");
                    WeaponBar weaponBar = kvp.Value;
                    weaponBar.UpdateTheme(emptyBar, fullBar);
                }
                else
                {
                    Log.Error("emptyBar is null in OnChanged");
                }
            }
        }

        private void UpdateWidth(MainSystem sys)
        {
            // This is straightforward.
            // Set the width of the container based on the selected option.
            MainContainer mainContainer = sys.state.container;

            mainContainer.Width.Pixels = SizeHelper.WidthSizes[Conf.C.PanelWidth];
            mainContainer.panel.Width.Pixels = SizeHelper.WidthSizes[Conf.C.PanelWidth];
            sys.state.container.Recalculate();
            // Also update the bar asset, otherwise it will look stretched out and ugly.
            // Meaning its 300 px version or 400 px version etc.
        }

        private void UpdateHeight(MainSystem sys)
        {
            MainPanel panel = sys.state.container.panel;

            // Look up the new bar height based on your config's "Height" value:
            float newHeight = SizeHelper.HeightSizes[Conf.C.BarHeight];

            // 1) Update the MainPanel's ItemHeight so newly created bars will match
            panel.ItemHeight = newHeight;

            // 2) Update each existing bar (both player bars & weapon bars)
            //    so they change their own Height property.
            foreach (var pb in panel.PlayerBars.Values)
            {
                pb.SetItemHeight(newHeight);
            }
            foreach (var wb in panel.WeaponBars.Values)
            {
                wb.SetItemHeight(newHeight);
            }

            // 3) Re-build the layout so the panel repositions bars and resizes itself
            panel.RebuildAllBarsLayout();
        }

        public static class Conf
        {
            // C = instance
            // Example usage: Conf.C.ShowPlayerIcons...
            public static Config C => ModContent.GetInstance<Config>();
        }
    }
}
