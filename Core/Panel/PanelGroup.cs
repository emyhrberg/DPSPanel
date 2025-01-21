using Microsoft.Xna.Framework;
using Terraria;
using Terraria.UI;

namespace DPSPanel.Core.Panel
{
    /// <summary>
    /// Parent container that holds both the Panel and KingSlimeButton
    /// so they move together.
    /// </summary>
    public class PanelGroup : UIElement
    {
        public Panel panel;
        public KingSlimeButton ksButton;

        private readonly float PANEL_WIDTH = 300f; // PANEL WIDTH
        private readonly float PANEL_HEIGHT = 40f; // PANEL HEIGHT. is reset later anyways

        // For dragging the entire group
        private bool dragging;
        private Vector2 dragOffset;

        // Where this group is anchored on the screen
        private Vector2 position;

        // Is the panel currently visible
        private bool panelVisible = true;

        // Constructor
        public PanelGroup()
        {
            Width.Set(PANEL_WIDTH, 0f);
            Height.Set(PANEL_HEIGHT, 0f);
            VAlign = 0.5f;
            HAlign = 0.5f;

            panel = new Panel();
            Append(panel);

            // 2) Create the King Slime button
            ksButton = new KingSlimeButton();
            ksButton.Left.Set(310f, 0f); // Place it to the right of the panel
            Append(ksButton);

            Append(ksButton);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // If dragging, follow the mouse
            if (dragging)
            {
                position = new Vector2(Main.mouseX, Main.mouseY) - dragOffset;
                ClampToScreen();
            }

            // Force the Panel to be at this group's position
            panel.Left.Set(position.X, 0f);
            panel.Top.Set(position.Y, 0f);
            panel.Recalculate();

            // Position the button a bit to the right (or wherever you want)
            Vector2 buttonPos = position + new Vector2(60, 0);
            ksButton.Left.Set(buttonPos.X, 0f);
            ksButton.Top.Set(buttonPos.Y, 0f);
            ksButton.Recalculate();
        }

        public void StartDrag()
        {
            dragging = true;
            dragOffset = new Vector2(Main.mouseX, Main.mouseY) - position;
            Main.LocalPlayer.mouseInterface = true;
        }

        public void StopDrag()
        {
            dragging = false;
        }

        public void TogglePanel()
        {
            panelVisible = !panelVisible;
            if (panelVisible)
                Append(panel);
            else
                panel.Remove();
        }

        private void ClampToScreen()
        {
            // Basic clamp so the group won't go off-screen
            position.X = Utils.Clamp(position.X, 0, Main.screenWidth);
            position.Y = Utils.Clamp(position.Y, 0, Main.screenHeight);
        }
    }
}
