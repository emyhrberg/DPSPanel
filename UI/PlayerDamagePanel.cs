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

namespace DPSPanel.UI
{
    public class PlayerDamagePanel : UIPanel
    {
        private readonly float PANEL_PADDING = 5f; // No extra padding on the panel
        private readonly float ITEM_PADDING = 10f;   // Vertical spacing between weapon bars
        private float currentYOffset = 0f;           // Y offset for each new weapon bar
        private const float ItemHeight = 16f;        // Height of each weapon bar

        public bool IsVisible;

        // Dictionary mapping weapon names to their corresponding WeaponBar.
        private Dictionary<string, WeaponBar> weaponBars = new Dictionary<string, WeaponBar>();

        public PlayerDamagePanel()
        {
            Width.Set(150, 0f);

            // Start with a minimal height (will be updated by UpdateWeaponBars).
            Height.Set(40f, 0f);
            BackgroundColor = new Color(27, 29, 85); // Dark blue background.
            SetPadding(PANEL_PADDING);
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
                Color color = PanelColors.colors[i % PanelColors.colors.Length];
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
