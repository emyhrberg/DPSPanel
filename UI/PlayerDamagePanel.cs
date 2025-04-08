using System.Collections.Generic;
using System.Linq;
using DPSPanel.Common.Configs;
using DPSPanel.Common.DamageCalculation;
using DPSPanel.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;
using static DPSPanel.Common.Configs.Config;

namespace DPSPanel.UI
{
    public class PlayerDamagePanel : UIPanel
    {
        private float PANEL_PADDING = 5f; // No extra padding on the panel
        private float ITEM_PADDING = 10f;   // Vertical spacing between weapon bars
        private float currentYOffset = 0f;           // Y offset for each new weapon bar
        private float ItemHeight = 16f;        // Height of each weapon bar

        public bool IsVisible;

        // Dictionary mapping weapon names to their corresponding WeaponBar.
        private Dictionary<string, WeaponBar> weaponBars = new Dictionary<string, WeaponBar>();

        public PlayerDamagePanel()
        {
            float width = SizeHelper.GetWidthFromConfig();
            Width.Set(width, 0f);
            // Width.Set(150, 0f);

            // Other
            MaxHeight = new StyleDimension(1000f, 0f); // must be done to allow producing new bars

            // Offset to the left by one panel
            Left.Set(width, 0);

            // Start with a minimal height (will be updated by UpdateWeaponBars).
            Height.Set(40f, 0f);
            BackgroundColor = new Color(27, 29, 85); // Dark blue background.
            SetPadding(PANEL_PADDING);
        }

        public void UpdateBarHeight(float updatedHeight)
        {
            ItemHeight = updatedHeight;
            // Update the height of each weapon bar.
            foreach (var weaponBar in weaponBars.Values)
            {
                weaponBar.Height.Set(updatedHeight, 0f);
            }
        }

        public void UpdateLeft(float updatedLeftOffset)
        {
            Left.Set(updatedLeftOffset, 0);
        }

        public void UpdatePanelWidth(float updatedWidth)
        {
            Width.Set(updatedWidth, 0f);
        }

        public void CreateWeaponBar(string weaponName)
        {
            // Create a new WeaponBar instance and store it in the dictionary.
            WeaponBar damageBar = new WeaponBar(0f);
            weaponBars[weaponName] = damageBar;
            Append(damageBar);

            // Add padding to the panel's height for the new weapon bar.
            Height.Pixels += ItemHeight + ITEM_PADDING;
        }

        public void UpdateWeaponBars(List<Weapon> weapons)
        {
            // Reset the current offset at the start.
            currentYOffset = 0f;

            // Sort weapons by descending damage.
            var sortedWeapons = weapons.OrderByDescending(w => w.damage).ToList();

            // Determine the highest damage (avoid division by zero).
            int highestDamage = sortedWeapons.Count > 0 ? sortedWeapons.First().damage : 1;
            if (highestDamage == 0)
                highestDamage = 1;

            // Loop over each weapon in sorted order.
            for (int i = 0; i < sortedWeapons.Count; i++)
            {
                var wpn = sortedWeapons[i];
                if (!weaponBars.ContainsKey(wpn.weaponName))
                    CreateWeaponBar(wpn.weaponName);

                WeaponBar bar = weaponBars[wpn.weaponName];
                int percent = (int)((float)wpn.damage / highestDamage * 100);

                // Use the sorted index to assign a color.
                Color color = Color.White; // Default color
                if (Conf.C.BarColors == "Rainbow")
                {
                    color = ColorHelper.rainbowColors()[i % ColorHelper.rainbowColors().Length];
                }
                else
                {
                    color = ColorHelper.standardColors[i % ColorHelper.standardColors.Length];
                }
                bar.UpdateWeaponBar(percent, wpn.weaponName, wpn.damage, wpn.weaponItemID, color);

                // Position the weapon bar at the current offset.
                bar.Top.Set(currentYOffset, 0f);
                currentYOffset += ItemHeight + ITEM_PADDING * 2;
            }
            // Update the panel's height to fit all weapon bars.
            Height.Set(currentYOffset + ITEM_PADDING, 0f);
            Recalculate();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!IsVisible)
                return;

            base.Draw(spriteBatch);
        }
    }
}
