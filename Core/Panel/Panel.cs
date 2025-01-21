using System;
using System.Collections.Generic;
using System.Linq;
using DPSPanel.Core.Helpers;
using log4net.Repository.Hierarchy;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using static DPSPanel.Core.Panel.BossDamageTrackerSP;

namespace DPSPanel.Core.Panel
{
    public class Panel : UIPanel
    {
        /* -------------------------------------------------------------
         * Variables
         * -------------------------------------------------------------
         */

        // Panel
        private readonly float padding = 5f;
        private readonly float w = 300f; // PANEL WIDTH
        private readonly float h = 40f; // PANEL HEIGHT. 2is reset later anyways
        private readonly Color panelColor = new(49, 84, 141); // Light blue, same as inventory panel

        // Panel items
        private float currentYOffset = 0;
        private const float ItemHeight = 40f;

        // Slider items
        private Dictionary<string, PanelSlider> sliders = [];

        // Header
        private NPC currentBoss;
        private const float headerHeight = 16f;

        private readonly Color[] colors =
        [
            Color.Red,
            Color.Green,
            Color.Blue,
            Color.Yellow,
            Color.Purple,
            Color.Orange,
            Color.Cyan,
            Color.Pink,
            Color.LightGreen,
            Color.LightBlue,
            Color.LightCoral,
            Color.LightGoldenrodYellow,

        ];

        public Panel()
        {
            SetPadding(padding);

            // Size of panel
            Width.Set(w, 0f);
            Height.Set(h, 0f);

            // Center the panel on the screen
            VAlign = 0.07f; // vertical pos. 
            HAlign = 0.5f;

            // Background color of panel
            BackgroundColor = panelColor;
        }

        /* -------------------------------------------------------------
         * Panel content
         * -------------------------------------------------------------
         */
        public void AddBossTitle(string bossName = "Boss Name", NPC npc = null)
        {
            currentBoss = npc;
            UIText bossTitle = new(bossName, 1.0f);
            bossTitle.HAlign = 0.5f;
            Append(bossTitle);

            currentYOffset = headerHeight + padding * 2; // Adjust Y offset for the next element
            ResizePanelHeight();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            ModContent.GetInstance<DPSPanel>().Logger.Info($"Curentt boss in panel: {currentBoss}");

            // Draw boss head icon if a boss is active
            if (currentBoss != null)
            {
                int headIndex = currentBoss.GetBossHeadTextureIndex();
                if (headIndex >= 0 && headIndex < TextureAssets.NpcHeadBoss.Length &&
                    TextureAssets.NpcHeadBoss[headIndex]?.IsLoaded == true)
                {
                    // Get the boss head texture
                    Texture2D bossHeadTexture = TextureAssets.NpcHeadBoss[headIndex].Value;

                    // Calculate the icon's position (top-left aligned within the panel)
                    CalculatedStyle dims = GetDimensions();
                    Vector2 iconPosition = new Vector2(
                        dims.X + 4f,
                        dims.Y - 2f
                    );

                    // Draw the boss head icon
                    spriteBatch.Draw(bossHeadTexture, iconPosition, Color.White);
                }
            }
        }

        public void CreateSlider(string sliderName = "Name")
        {
            // Check if the slider already exists
            if (!sliders.ContainsKey(sliderName))
            {
                // Create a new slider
                PanelSlider slider = new(currentYOffset);
                Append(slider);
                currentYOffset += ItemHeight + padding * 2; // Adjust Y offset for the next element
                ResizePanelHeight();

                // Add to the dictionary
                sliders[sliderName] = slider;
            }
        }

        public void UpdateSliders(List<Weapon> weapons)
        {
            // Reset vertical offset.
            currentYOffset = headerHeight + padding * 2;

            // Sort weapons by descending damage.
            weapons = weapons.OrderByDescending(w => w.damage).ToList();
            int highest = weapons.FirstOrDefault()?.damage ?? 1;

            for (int i = 0; i < weapons.Count; i++)
            {
                var wpn = weapons[i];
                Color color = colors[i % colors.Length];

                // Get the slider for this weapon.
                PanelSlider slider = sliders[wpn.weaponName];

                // Update the slider with the current data.
                slider.UpdateSlider(highest, wpn.weaponName, wpn.damage, color, wpn.itemID, wpn.itemType);
                slider.Top.Set(currentYOffset, 0f);
                currentYOffset += ItemHeight + padding * 2;
            }
            ResizePanelHeight();
        }

        public void ClearPanelAndAllItems()
        {
            RemoveAllChildren();
            sliders = []; // reset sliders
        }

        private void ResizePanelHeight()
        {
            Height.Set(currentYOffset + padding, 0f);
            Recalculate();
        }
    }
}