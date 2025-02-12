using System.Collections.Generic;
using System.Linq;
using DPSPanel.Core.Configs;
using DPSPanel.Core.DamageCalculation;
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
            // Reset the Y offset.
            currentYOffset = 0f;
            // Determine the highest damage among the weapons.
            int highestDamage = weapons.Count > 0 ? weapons.Max(w => w.damage) : 1;
            if (highestDamage == 0)
                highestDamage = 1;

            // Loop through each weapon in the list.
            foreach (var wpn in weapons)
            {
                // If a bar for this weapon doesn't exist yet, create it.
                if (!damageBars.ContainsKey(wpn.weaponName))
                    CreateWeaponBar(wpn.weaponName);

                // Retrieve and update the corresponding weapon bar.
                WeaponBar bar = damageBars[wpn.weaponName];
                int percent = (int)((float)wpn.damage / highestDamage * 100);
                bar.UpdateWeaponBar(percent, wpn.weaponName, wpn.damage, wpn.weaponItemID, Color.White);
                // Position this bar vertically.
                bar.Top.Set(currentYOffset, 0f);
                currentYOffset += ItemHeight + ITEM_PADDING;
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

        public override void Update(GameTime gameTime)
        {
            if (!IsVisible)
                return;
            base.Update(gameTime);
        }
    }
}
