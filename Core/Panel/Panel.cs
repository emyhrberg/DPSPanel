using System;
using System.Collections.Generic;
using System.Linq;
using DPSPanel.Core.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
        // Panel
        private readonly float padding = 5f;
        private readonly float PANEL_WIDTH = 300f; // 300 width
        private readonly float PANEL_HEIGHT = 40f; // is reset anyways
        private readonly Color panelColor = new(49, 84, 141); 
        private float currentYOffset = 0;
        private const float ItemHeight = 40f;

        // Boss icon button
        private BossIconElement bossIconButton;

        // Slider items
        private Dictionary<string, PanelSlider> sliders = [];
        private NPC currentBoss;
        private const float headerHeight = 16f;

        public Panel()
        {
            Width.Set(PANEL_WIDTH, 0f);
            Height.Set(PANEL_HEIGHT, 0f);
            BackgroundColor = panelColor;
            VAlign = 0.07f; // 7% from the top
            HAlign = 0.5f; // Center horizontally
            SetPadding(padding);

            // Add boss icon button with a default texture
            bossIconButton = new BossIconElement();
            Append(bossIconButton);
        }

        public void AddBossTitle(string bossName = "Boss Name", NPC npc = null)
        {
            currentBoss = npc;
            UIText bossTitle = new(bossName, 1.0f);
            bossTitle.HAlign = 0.5f;
            Append(bossTitle);

            // add boss icon
            bossIconButton.UpdateBossIcon(npc);

            currentYOffset = headerHeight + padding * 2; // Adjust Y offset for the next element
            ResizePanelHeight();
        }

        public override void Draw(SpriteBatch sb)
        {
            base.Draw(sb);
            DrawBossIconDuringFight(sb); 
        }
        private void DrawBossIconDuringFight(SpriteBatch sb)
        {
            if (currentBoss != null)
            {
                int i = currentBoss.GetBossHeadTextureIndex();
                Texture2D bossHeadTexture = TextureAssets.NpcHeadBoss[i]?.Value;
                CalculatedStyle dims = GetInnerDimensions();
                Vector2 pos = new(dims.X, dims.Y);
                sb.Draw(bossHeadTexture, pos, Color.White);
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
                sliders[sliderName] = slider;

                currentYOffset += ItemHeight + padding * 2; // Adjust Y offset for the next element
                ResizePanelHeight();
            }
        }

        public void UpdateSliders(List<Weapon> weapons)
        {
            // Reset vertical offset. needed?
            currentYOffset = headerHeight + padding * 2;

            // Sort weapons by descending damage.
            weapons = weapons.OrderByDescending(w => w.damage).ToList();
            int highest = weapons.FirstOrDefault()?.damage ?? 1;

            for (int i = 0; i < weapons.Count; i++)
            {
                var wpn = weapons[i];
                Color color = PanelColors.colors[i % PanelColors.colors.Length];

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