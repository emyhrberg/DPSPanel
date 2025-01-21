using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.UI;

namespace DPSPanel.Core.Panel
{
    public class BossPanelContainer : UIElement
    {
        public BossIconElement bossIcon;
        public Panel panel;

        private bool dragging;
        private Vector2 offset;

        public bool panelVisible = true;

        private const float PANEL_WIDTH = 300f;

        public BossPanelContainer()
        {
            // Container defaults
            Width.Set(PANEL_WIDTH, 0f);
            Height.Set(0f, 0f); // We'll adjust after measuring the panel
            VAlign = 0.07f; // 7% down from top
            HAlign = 0.5f;  // center horizontally

            // 1) Create the panel
            panel = new Panel();
            // Put the panel at 0,0 in local space
            panel.Left.Set(0f, 0f);
            panel.Top.Set(0f, 0f);

            // Whenever the panel changes size, we measure it
            panel.OnSizeChanged += OnPanelResized;
            Append(panel);

            // 2) Create the icon
            bossIcon = new BossIconElement();
            // Also at 0,0, on top of the panel
            bossIcon.Left.Set(0f, 0f);
            bossIcon.Top.Set(0f, 0f);
            // Append it last, so it draws on top
            Append(bossIcon);

            // Example usage
            panel.SetBossTitle("Fight a boss to display stats!", null);

            // Ensure we measure the panel right away
            AdjustSizeToPanel();
        }

        #region Resizing
        private void OnPanelResized()
        {
            AdjustSizeToPanel();
        }

        /// <summary>
        /// Adjusts the container size to match the panel's size (ignoring the icon).
        /// </summary>
        private void AdjustSizeToPanel()
        {
            CalculatedStyle containerDims = GetDimensions();
            CalculatedStyle panelDims = panel.GetDimensions();

            // Convert panel’s absolute bottom to local coords
            float localBottom = (panelDims.Y + panelDims.Height) - containerDims.Y;
            // If you also want to maintain a minimum height (like 40f), do:
            float finalHeight = MathHelper.Max(localBottom, 40f);

            // Guarantee container width is at least PANEL_WIDTH
            Width.Set(PANEL_WIDTH, 0f);
            // Use the panel’s measured height
            Height.Set(finalHeight, 0f);

            Recalculate();
        }
        #endregion

        #region Draggable Container (Entire Area)
        public override void LeftMouseDown(UIMouseEvent evt)
        {
            base.LeftMouseDown(evt);

            // If you want to start dragging whenever the user clicks on *any part* 
            // (including children), check ContainsPoint, not evt.Target == this.
            if (ContainsPoint(evt.MousePosition))
            {
                dragging = true;
                offset = new Vector2(evt.MousePosition.X - Left.Pixels, evt.MousePosition.Y - Top.Pixels);
            }
        }

        public override void LeftMouseUp(UIMouseEvent evt)
        {
            base.LeftMouseUp(evt);
            dragging = false;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // If the mouse is over any part of this container, block item usage
            if (ContainsPoint(Main.MouseScreen))
            {
                Main.LocalPlayer.mouseInterface = true;
            }

            // Dragging logic
            if (dragging)
            {
                Left.Set(Main.mouseX - offset.X, 0f);
                Top.Set(Main.mouseY - offset.Y, 0f);
                ClampToScreen();
                Recalculate();
            }
        }

        /// <summary>
        /// Clamps the container to the screen so it cannot be dragged off.
        /// </summary>
        private void ClampToScreen()
        {
            CalculatedStyle dims = GetDimensions();

            float clampedLeft = Utils.Clamp(Left.Pixels, 0f, Main.screenWidth - dims.Width);
            float clampedTop = Utils.Clamp(Top.Pixels, 0f, Main.screenHeight - dims.Height);

            Left.Set(clampedLeft, 0f);
            Top.Set(clampedTop, 0f);
        }
        #endregion

        #region Show/Hide Panel
        public void TogglePanel()
        {
            panelVisible = !panelVisible;

            if (panelVisible)
            {
                if (!Children.Contains(panel))
                {
                    Append(panel);
                    // remove & re-append the icon so it draws on top
                    bossIcon.Remove();
                    Append(bossIcon);
                    AdjustSizeToPanel();
                }
            }
            else
            {
                if (Children.Contains(panel))
                {
                    panel.Remove();
                    // If you want the container to shrink to just the icon’s size, do so:
                    Height.Set(bossIcon.GetDimensions().Height, 0f);
                    Recalculate();
                }
            }
        }
        #endregion
    }
}
