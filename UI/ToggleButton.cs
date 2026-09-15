using DPSPanel.Common.Configs;
using DPSPanel.Common.DamageCalculation;
using DPSPanel.Core.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace DPSPanel.UI
{
    public class ToggleButton : UIElement
    {
        private Texture2D img;
        private Texture2D imgHighlighted;
        private Vector2 clickStartPosition; // Start position of a mouse click
        private bool isDragging;

        public ToggleButton()
        {
            ApplyLayout();

            img = Ass.ToggleButton.Value;
            imgHighlighted = Ass.ToggleButtonHighlighted.Value;
        }

        public void ApplyLayout()
        {
            Width.Set(System.Math.Max(1, DPSPanelLayout.ToggleWidth), 0);
            Height.Set(System.Math.Max(1, DPSPanelLayout.ToggleHeight), 0);
            MaxHeight.Set(float.MaxValue, 0);
            Top.Set(DPSPanelLayout.ToggleTop, 0);
            Left.Set(DPSPanelLayout.ToggleLeft, 0);
            Recalculate();
        }

        protected override void DrawSelf(SpriteBatch sb)
        {

            MainSystem sys = ModContent.GetInstance<MainSystem>();
            bool showOnlyWhenInventoryOpen = sys.state.container.panel.HideWhenInventoryOpen;

            if (!Main.playerInventory && !showOnlyWhenInventoryOpen)
                return;

            base.DrawSelf(sb);

            // Get the dimensions of the element
            float scale = System.Math.Max(0.01f, DPSPanelLayout.ToggleIconScale);
            CalculatedStyle dims = GetDimensions();
            Vector2 pos = new(dims.X + (dims.Width - img.Width * scale) / 2f,
                              dims.Y + (dims.Height - img.Height * scale) / 2f);


            // Draw either the button or highlighted button based on hover state
            Config c = ModContent.GetInstance<Config>();
            if (IsMouseHovering && c.ShowTooltips)
            {
                sb.Draw(imgHighlighted, pos, null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
                Main.instance.MouseText("Left click to toggle panel \nRight click to only show when inventory is open\nAlt click to open config\nCtrl click to clear panel");
            }
            else
            {
                sb.Draw(img, pos, null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            }
        }

        #region RightClick
        public override void RightMouseDown(UIMouseEvent evt)
        {
            base.RightMouseDown(evt);

            // on right click we toggle the config setting to only show in inventory.
            MainSystem sys = ModContent.GetInstance<MainSystem>();
            sys.state.container.panel.HideWhenInventoryOpen = !sys.state.container.panel.HideWhenInventoryOpen;
            bool hideWhenInventoryOpen = sys.state.container.panel.HideWhenInventoryOpen;

            string text = hideWhenInventoryOpen ? "Always show DPSPanel" : "Show DPSPanel only when inventory is open";
            Main.NewText(text, Color.White);
        }
        #endregion

        #region ClickDragHotFix
        public override void LeftMouseDown(UIMouseEvent evt)
        {
            base.LeftMouseDown(evt);

            // Record the start position when the mouse is pressed
            clickStartPosition = evt.MousePosition;
            isDragging = false; // Reset dragging flag
        }

        public override void LeftMouseUp(UIMouseEvent evt)
        {
            base.LeftMouseUp(evt);

            // Check if the mouse moved significantly during the click
            if (Vector2.Distance(clickStartPosition, evt.MousePosition) > DPSPanelLayout.DragThreshold)
            {
                isDragging = true;
            }

            // Only toggle the panel if it was not a drag
            if (!isDragging)
            {
                var parentContainer = Parent as MainContainer;

                // Ensure the parent container is not null before accessing it
                if (parentContainer == null)
                {
                    Log.Warn("Parent container is null. TogglePanel() cannot be called.");
                    return;
                }

                // check if ctrl is pressed
                if (Main.keyState.IsKeyDown(Keys.LeftControl))
                {
                    // clear
                    MainSystem sys = ModContent.GetInstance<MainSystem>();
                    sys.ClearDisplay();
                    return;
                }

                // check if alt is pressed
                if (Main.keyState.IsKeyDown(Keys.LeftAlt))
                {
                    // open the config menu
                    var conf = ModContent.GetInstance<Config>();
                    if (conf != null)
                    {
                        conf.Open();
                    }
                    return;
                }
                parentContainer.TogglePanel();
            }
        }
        #endregion
    }
}
