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

        public bool dragging;
        private Vector2 offset;

        public bool panelVisible = true;

        private const float MIN_WIDTH = 300f;  // Minimum container width
        private const float MIN_HEIGHT = 40f;  // Minimum container height

        public BossPanelContainer()
        {
            // Container defaults
            Width.Set(MIN_WIDTH, 0f);
            Height.Set(MIN_HEIGHT, 0f);
            VAlign = 0.07f; // 7% down from top
            HAlign = 0.5f;  // center horizontally

            // 1) Create the panel
            panel = new Panel();
            panel.Left.Set(0f, 0f);
            panel.Top.Set(0f, 0f);
            Append(panel);

            // 2) Create the icon
            bossIcon = new BossIconElement();
            bossIcon.Left.Set(0f, 0f);
            bossIcon.Top.Set(0f, 0f);
            // Append it last, so it draws on top
            Append(bossIcon);

            panel.SetBossTitle("Fight a boss to display stats!");
        }

        #region Dragging
        public override void LeftMouseDown(UIMouseEvent evt)
        {
            base.LeftMouseDown(evt);
            if (ContainsPoint(evt.MousePosition))
                DragStart(evt);
        }

        public override void LeftMouseUp(UIMouseEvent evt)
        {
            base.LeftMouseUp(evt);
            if (dragging)
                DragEnd(evt);
        }

        private void DragStart(UIMouseEvent evt)
        {
            // Save an offset from the top-left corner of this container
            // var dims = GetDimensions().ToRectangle();
            offset = new Vector2(evt.MousePosition.X - Left.Pixels, evt.MousePosition.Y - Top.Pixels);
            dragging = true;
        }

        private void DragEnd(UIMouseEvent evt)
        {
            Vector2 endMousePosition = evt.MousePosition;
            dragging = false;

            Left.Set(endMousePosition.X - offset.X, 0f);
            Top.Set(endMousePosition.Y - offset.Y, 0f);

            Recalculate();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // This ensures we do not use other items while dragging
            if (ContainsPoint(Main.MouseScreen))
                Main.LocalPlayer.mouseInterface = true;

            if (dragging)
            {
                Left.Set(Main.mouseX - offset.X, 0f);
                Top.Set(Main.mouseY - offset.Y, 0f);
                Recalculate();
            }

            // Check if the container is out of bounds
            var parentSpace = Parent.GetDimensions().ToRectangle();
            if (!GetDimensions().ToRectangle().Intersects(parentSpace))
            {
                Left.Pixels = Utils.Clamp(Left.Pixels, 0, parentSpace.Right - Width.Pixels);
                Top.Pixels = Utils.Clamp(Top.Pixels, 0, parentSpace.Bottom - Height.Pixels);
                // Recalculate forces the UI system to do the positioning math again.
                Recalculate();
            }
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
                }
            }
            else
            {
                if (Children.Contains(panel))
                {
                    panel.Remove();
                }
            }
        }
        #endregion
    }
}
