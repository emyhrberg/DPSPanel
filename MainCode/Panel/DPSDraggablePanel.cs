using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;
using static DPSPanel.MainCode.Panel.BossDamageTracker;

namespace DPSPanel.MainCode.Panel
{
    public class DPSDraggablePanel : UIPanel
    {
        // Variables for dragging the panel
        private Vector2 offset;
        private bool dragging;
        private bool clickStartedInsidePanel;

        // Panel items
        private const float ItemHeight = 22f;
        private const float Padding = 10f; // Padding for the panel
        private float currentYOffset = 12f; // Initial offset for the first row

        // Dictionary to store unique player-to-color mappings
        private Dictionary<string, Color> playerColorMap = new Dictionary<string, Color>();

        // Define predefined colors for players
        private Color[] predefinedColors = new Color[]
        {
            new Color(240, 85, 85),   // Warm Red
            new Color(85, 115, 240), // Cool Blue
            new Color(255, 140, 0),  // Vivid Orange
            new Color(60, 180, 170), // Teal
            new Color(255, 215, 70)  // Gold
        };

        // Store the labels for each boss
        private Dictionary<string, UIText> bossLabels = new Dictionary<string, UIText>();

        public override void OnInitialize()
        {
            AddHeaderTextToPanel("DPS Panel (press K to toggle)");
            ResizeToFitItems();
        }

        public void UpdateDPSPanel(BossFight fight)
        {
            // Update boss entry
            string bossKey = $"Boss:{fight.bossId}";
            string bossText = $"{fight.bossName} - {fight.damageTaken} damage";
            Color bossColor = new Color(220, 220, 220);
            UpdateItem(bossKey, bossText, bossColor);

            // Update player and weapon entries
            foreach (var plr in fight.players)
            {
                // Check if the player already has a color assigned
                if (!playerColorMap.ContainsKey(plr.playerName))
                {
                    // Assign the next available predefined color, cycling if necessary
                    int currentCount = playerColorMap.Count;
                    playerColorMap[plr.playerName] = predefinedColors[currentCount % predefinedColors.Length];
                }

                // Get the assigned color for the player
                Color playerColor = playerColorMap[plr.playerName];

                string playerKey = $"{fight.bossId}|Player:{plr.playerName}";
                string playerText = $"    {plr.playerName} - {plr.totalDamage} damage";
                UpdateItem(playerKey, playerText, playerColor);

                // Use the same color for each weapon of this player
                foreach (var wpn in plr.weapons)
                {
                    string weaponKey = $"Boss:{fight.bossId}|Player:{plr.playerName}|Weapon:{wpn.weaponName}";
                    string weaponText = $"    {wpn.weaponName} - {wpn.damage} damage";
                    UpdateItem(weaponKey, weaponText, playerColor);
                }
            }
        }


        private void AddHeaderTextToPanel(string text)
        {
            var label = new UIText(text);
            label.Left.Set(30f, 0f);
            label.TextColor = new Color(255,255,255);
            Append(label);
        }

        public void UpdateItem(string key, string text, Color color)
        {
            // If we already have a label for this key, update its text
            if (bossLabels.ContainsKey(key))
            {
                bossLabels[key].SetText(text);
                bossLabels[key].TextColor = color;
            }
            // Otherwise, create a new label
            else
            {
                var label = new UIText(text);
                label.Top.Set(currentYOffset, 0f);
                label.TextColor = color;

                bossLabels.Add(key, label);
                Append(label);
                ResizeToFitItems();
            }
        }

        private void ResizeToFitItems()
        {
            currentYOffset += ItemHeight + 2f; // extra for padding
            Height.Set(currentYOffset + Padding, 0f);
            Recalculate();
        }

        public void ClearItems()
        {
            // Remove only the items in bossLabels
            foreach (var kvp in bossLabels)
            {
                kvp.Value.Remove();
            }
            bossLabels.Clear();

            // Reset the Y offset to below the header
            currentYOffset = 30f;
            ResizeToFitItems();
        }

        /*
         * DRAGGING FUNCTIONALITY
         */////////////////////////////////////////////////////////////

        public override void LeftMouseDown(UIMouseEvent evt)
        {
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
            // Start dragging the panel
            offset = new Vector2(evt.MousePosition.X - Left.Pixels, evt.MousePosition.Y - Top.Pixels);
            dragging = true;
        }

        private void DragEnd(UIMouseEvent evt)
        {
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
