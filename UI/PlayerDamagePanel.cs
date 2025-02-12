using System.Collections.Generic;
using System.Linq;
using DPSPanel.Core.Configs;
using DPSPanel.Core.DamageCalculation;
using DPSPanel.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;

namespace DPSPanel.UI
{
    public class PlayerDamagePanel : UIPanel
    {
        private readonly float PANEL_PADDING = 5f; // Padding inside the panel
        private readonly float ITEM_PADDING = 10f;   // Vertical spacing between weapon bars
        private float currentYOffset = 0f;           // Y offset for each new weapon bar
        private const float ItemHeight = 40f;        // Height of each weapon bar

        // Visibility flag.
        public bool IsVisible = true;

        // Dictionary mapping weapon names to their corresponding WeaponBar.
        private Dictionary<string, WeaponBar> damageBars = new Dictionary<string, WeaponBar>();

        public PlayerDamagePanel()
        {
            Config c = ModContent.GetInstance<Config>();
            if (c.BarWidth == 150)
                Width.Set(150, 0f);
            else if (c.BarWidth == 300)
                Width.Set(300, 0f);

            // Start with a minimal height (will be updated).
            Height.Set(300f, 0f);
            BackgroundColor = new Color(27, 29, 85); // Dark blue background.
            SetPadding(PANEL_PADDING);
            OverflowHidden = false; // Allow overflow from weapon bars.
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
            // Sort weapons by descending damage.
            var sortedWeapons = weapons.OrderByDescending(w => w.damage).ToList();

            // Determine the highest damage.
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
                bar.Top.Set(currentYOffset, 0f);
                currentYOffset += ItemHeight + ITEM_PADDING;
            }
            // Update this panel's height to fit all weapon bars.
            Height.Set(currentYOffset + ITEM_PADDING, 0f);
            Log.Info($"Updated player damage panel height to {currentYOffset + ITEM_PADDING}");
            Recalculate();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!IsVisible)
                return;
            base.Draw(spriteBatch);
        }

        public override void Update(GameTime gameTime)
        {
            if (!IsVisible)
                return;
            base.Update(gameTime);
        }
    }
}
