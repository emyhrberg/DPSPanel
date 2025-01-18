using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;

namespace DPSPanel.MainCode.Panel
{
    public class Panel : UIPanel
    {
        /* -------------------------------------------------------------
         * Variables
         * -------------------------------------------------------------
         */
        // Variables for dragging the panel
        private Vector2 offset;
        private bool dragging;
        private bool clickStartedInsidePanel;
        private readonly bool IS_DRAGGABLE = false; // Debug option: Set to true to enable dragging

        // Panel items
        private readonly float padding;
        private readonly float headerHeight = 16f;
        private float currentYOffset = 0;
        private const float ItemHeight = 40f;

        // Slider items
        private PanelSlider slider;
        private readonly Asset<Texture2D> sliderEmpty;
        private readonly Asset<Texture2D> sliderFull;

        /* -------------------------------------------------------------
         * Panel Constructor
         * -------------------------------------------------------------
         */
        public Panel(float padding)
        {
            this.padding = padding;
            SetPadding(padding);
            sliderEmpty = ModContent.Request<Texture2D>("DPSPanel/MainCode/Assets/SliderEmpty");
            sliderFull = ModContent.Request<Texture2D>("DPSPanel/MainCode/Assets/SliderFull");
        }

        // Define predefined colors for each row
        private readonly Color[] colorsToUse =
        [
            //new Color(255, 140, 0),  // Vivid Orange
            //new Color(242,206,109), // Gold v2
            //new Color(207,195,191), // Silver-ish v2
            //new Color(86,70,71), // Silver-ish
            //new Color(65,37,8), // Bronze-ish
            new Color(255, 215, 70),  // Gold
            new Color(240, 85, 85),   // Warm Red
            new Color(85, 115, 240), // Cool Blue
            new Color(60, 180, 170), // Teal
            new Color(186,137,87), // Bronze-ish v2
            new Color(255, 140, 0),  // Vivid Orange
            new Color(242,206,109), // Gold v2
            new Color(207,195,191), // Silver-ish v2

        ];
        private int colorIndex;

        /* -------------------------------------------------------------
         * Panel content
         * -------------------------------------------------------------
         */

        public void AddDefaultPanelHeader(string text="Damage dealt")
        {
            // Header text
            UIText header = new(text, 1.0f);
            header.HAlign = 0.5f; // center horizontal align
            Append(header);
            currentYOffset = headerHeight + padding*2; // add padding on both sides to the height
            ResizePanelHeight();
        }

        public void AddBossTitle(string bossName="UnnamedBoss")
        {
            // Clear the entire panel
            RemoveAllChildren();
            slider = null;

            UIText bossTitle = new(bossName, 1.0f);
            bossTitle.HAlign = 0.5f;
            Append(bossTitle);
            currentYOffset = headerHeight + padding * 2; // Adjust Y offset for the next element
            ResizePanelHeight();
        }

        public void CreateSlider()
        {
            // Check if slider exists
            if (slider == null)
            {
                // Select unused color
                // More likely to select first color
                // Very likely to select second or third
                // Percentages: 1. 80%, 2/3. 10%, 4/5. 5%
                Random rnd = new Random();
                if (rnd.Next(1, 101) <= 80)
                {
                    colorIndex = 0;
                }
                else if (rnd.Next(1, 101) <= 10)
                {
                    colorIndex = 1;
                }
                else if (rnd.Next(1, 101) <= 5)
                {
                    colorIndex = 2;
                }
                else
                {
                    colorIndex = rnd.Next(3, colorsToUse.Length);
                }


                Color color = colorsToUse[colorIndex++ % colorsToUse.Length];
                // Create a slider
                slider = new(sliderEmpty, sliderFull, Main.LocalPlayer.name, color, 0)
                {
                    Width = new StyleDimension(0, 1.0f), // Fill the width of the panel
                    Height = new StyleDimension(ItemHeight, 0f), // Set height
                    Top = new StyleDimension(currentYOffset, 0f),
                    HAlign = 0.5f, // Center horizontally
                };
                Append(slider);
                currentYOffset += ItemHeight + padding * 2; // Adjust Y offset for the next element
                ResizePanelHeight();
            }
        }

        public void UpdateSlider(int damageDone, int percentageValue)
        {
            // Ensure slider exists
            if (slider != null)
            {
                // Update the slider value
                slider.updateSliderValue(damageDone, percentageValue);
                Recalculate();
            }
        }

        private void ResizePanelHeight()
        {
            Height.Set(currentYOffset + padding, 0f);
            Recalculate();
        }

        /* -------------------------------------------------------------
         * Dragging functionality
         * -------------------------------------------------------------
         */

        public override void LeftMouseDown(UIMouseEvent evt)
        {

            if (!IS_DRAGGABLE) return; 

            // When the mouse is pressed, start dragging the panel
            base.LeftMouseDown(evt);
            if (evt.Target == this)
            {
                DragStart(evt);
                clickStartedInsidePanel = true;

                // Prevent other UI elements from interacting
                Main.LocalPlayer.mouseInterface = true;
            }
        }

        public override void LeftMouseUp(UIMouseEvent evt)
        {
            if (!IS_DRAGGABLE) return;

            // When the mouse is released, stop dragging the panel
            base.LeftMouseUp(evt);
            if (clickStartedInsidePanel)
            {
                DragEnd(evt);
            }
            clickStartedInsidePanel = false; // default to false
        }

        private void DragStart(UIMouseEvent evt)
        {
            if (!IS_DRAGGABLE) return;

            // Start dragging the panel
            offset = new Vector2(evt.MousePosition.X - Left.Pixels, evt.MousePosition.Y - Top.Pixels);
            dragging = true;
        }

        private void DragEnd(UIMouseEvent evt)
        {
            if (!IS_DRAGGABLE) return;

            // Stop dragging the panel
            dragging = false;
            Vector2 endMousePosition = evt.MousePosition;
            Left.Set(endMousePosition.X - offset.X, 0f);
            Top.Set(endMousePosition.Y - offset.Y, 0f);
            Recalculate();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (!IS_DRAGGABLE) return;

            // If the mouse is inside the panel, set the mouse interface to true to prevent other UI elements from interacting
            if (dragging || clickStartedInsidePanel && ContainsPoint(Main.MouseScreen))
            {
                Main.LocalPlayer.mouseInterface = true;
            }

            // Drag the panel
            if (dragging)
            {
                Left.Set(Main.mouseX - offset.X, 0f);
                Top.Set(Main.mouseY - offset.Y, 0f);
                Recalculate();
            }

            // Keep the panel within bounds
            var parentSpace = Parent.GetDimensions().ToRectangle();
            if (!GetDimensions().ToRectangle().Intersects(parentSpace))
            {
                Left.Pixels = Utils.Clamp(Left.Pixels, 0, parentSpace.Right - Width.Pixels);
                Top.Pixels = Utils.Clamp(Top.Pixels, 0, parentSpace.Bottom - Height.Pixels);
                Recalculate();
            }
        }
    }
}
