using System.Collections.Generic;
using System.Linq;
using DPSPanel.Core.Configs;
using DPSPanel.Core.DamageCalculation;
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
        private readonly float ITEM_PADDING = 20f;   // Vertical spacing between weapon bars
        private float currentYOffset = 0f;           // Y offset for each new weapon bar
        private const float ItemHeight = 16f;        // Height of each weapon bar

        public bool IsVisible;

        // Dictionary mapping weapon names to their corresponding WeaponBar.
        private Dictionary<string, WeaponBar> damageBars = new Dictionary<string, WeaponBar>();

        // Reference to the associated player's bar.
        // (Be sure to set this from your state/parent code when you create this panel.)
        public PlayerBar OwnerBar { get; set; }

        public PlayerDamagePanel()
        {
            Config c = ModContent.GetInstance<Config>();
            if (c.BarWidth == 150)
                Width.Set(150, 0f);
            else if (c.BarWidth == 300)
                Width.Set(300, 0f);

            // Start with a minimal height (will be updated by UpdateWeaponBars).
            Height.Set(40f, 0f);
            BackgroundColor = new Color(27, 29, 85); // Dark blue background.
            SetPadding(PANEL_PADDING);
        }

        public void CreateWeaponBar(string weaponName)
        {
            // Create a new WeaponBar instance and store it in the dictionary.
            WeaponBar damageBar = new WeaponBar(0f);
            damageBars[weaponName] = damageBar;
            Append(damageBar);
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
                if (!damageBars.ContainsKey(wpn.weaponName))
                    CreateWeaponBar(wpn.weaponName);

                WeaponBar bar = damageBars[wpn.weaponName];
                int percent = (int)((float)wpn.damage / highestDamage * 100);
                // Use the sorted index to assign a color.
                Color color = PanelColors.colors[i % PanelColors.colors.Length];
                bar.UpdateWeaponBar(percent, wpn.weaponName, wpn.damage, wpn.weaponItemID, color);

                // Position the weapon bar at the current offset.
                bar.Top.Set(currentYOffset, 0f);
                currentYOffset += ItemHeight + ITEM_PADDING;
            }
            // Update the panel's height to fit all weapon bars.
            Height.Set(currentYOffset, 0f);
            Recalculate();
        }

        public override void Update(GameTime gameTime)
        {
            if (OwnerBar != null)
            {
                // Get the OwnerBar’s absolute (global) dimensions.
                // (GetDimensions() returns absolute coordinates when the UI is set up correctly.)
                CalculatedStyle ownerDims = OwnerBar.GetDimensions();

                // Choose an offset for the damage panel relative to the owner.
                // For example, 200 pixels to the right.
                float offsetX = 200f;
                float offsetY = 0f;

                // Set the damage panel’s position in global coordinates.
                // Because the damage panel is appended to the state, these positions are global.
                Left.Set(ownerDims.X + offsetX, 0f);
                Top.Set(ownerDims.Y + offsetY, 0f);
                Recalculate();

                // Optionally, update its visibility based on hover state.
                IsVisible = OwnerBar.IsMouseHovering || IsMouseHovering;
            }
            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!IsVisible)
                return;

            base.Draw(spriteBatch);
        }
    }
}
