using DPSPanel.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Renderers;
using Terraria.ModLoader;
using Terraria.UI;

namespace DPSPanel.UI
{
    [Autoload(Side = ModSide.Client)]
    public class PanelSystem : ModSystem
    {
        // Variables
        private UserInterface ui;
        internal PanelState state;

        private readonly Vector2 startPosition = new Vector2(20, 20); // Starting position for drawing heads
        private const int spacing = 50; // Spacing between player heads

        // Reflection-related fields
        private FieldInfo playerRendersField;
        private PlayerHeadDrawRenderTargetContent[] playerRenders;

        public override void Load()
        {
            LoadAssets.PreloadAllAssets();

            // Cache the private _playerRenders field using reflection
            Type mapPlayerRendererType = typeof(MapHeadRenderer);
            playerRendersField = mapPlayerRendererType.GetField("_playerRenders", BindingFlags.NonPublic | BindingFlags.Instance);
            if (playerRendersField == null)
            {
                ModContent.GetInstance<DPSPanel>().Logger.Warn("Failed to access _playerRenders field via reflection.");
            }
            else
            {
                // Get the _playerRenders array from the MapPlayerRenderer instance
                playerRenders = playerRendersField.GetValue(Main.MapPlayerRenderer) as PlayerHeadDrawRenderTargetContent[];
                if (playerRenders == null)
                {
                    ModContent.GetInstance<DPSPanel>().Logger.Warn("_playerRenders field is null.");
                }
            }
        }

        public override void PostSetupContent()
        {
            // This is called after everything in the game has been loaded
            state = new PanelState();
            ui = new UserInterface();

            state.Activate();
            ui.SetState(state);
            ModContent.GetInstance<DPSPanel>().Logger.Info("PanelSystem initialized!");

            // Test drawing player heads
            // state.container.panel.UpdateDamageBars($"PlayerName {'a'}", 300, 0);
            // state.container.panel.UpdateDamageBars($"PlayerName {'b'}", 500, 0);
        }

        public override void UpdateUI(GameTime gameTime)
        {
            ui?.Update(gameTime); // Always update the UI (everything in the PanelState, Panel, etc.)
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
            if (mouseTextIndex != -1)
            {
                layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                    "DPSPanel: UI System", // Debug layer
                    delegate
                    {
                        // test drawing here
                        // DrawPlayerHeads();

                        // Main UI system drawing
                        ui.Draw(Main.spriteBatch, new GameTime());
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
            }
        }

        public void DrawPlayerHeads()
        {
            // Define the size of each player head icon
            int headSize = 32;
            int padding = 5; // Space between the head and the damage bar

            // Starting position for the first player's head
            // Adjust X based on where you want the heads relative to the damage bars
            Vector2 startPosition = new Vector2(10, 50); // Example values

            // Iterate through all possible players
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player player = Main.player[i];

                // Skip inactive or null players
                if (player == null || !player.active)
                    continue;

                // Get the border color for the player's head
                Color headBordersColor = Main.GetPlayerHeadBordersColor(player);

                // Calculate the Y position based on the player's index in the sorted damage bars
                // Assuming damage bars are sorted and arranged vertically with consistent spacing
                float yPosition = 50 + i * (headSize + padding);

                // Define the position for the player's head
                Vector2 headPosition = new Vector2(startPosition.X, yPosition);

                // Draw the player's head using the existing MapPlayerRenderer
                Main.MapPlayerRenderer.DrawPlayerHead(
                    Main.Camera,      // Current camera
                    player,           // Player to draw
                    headPosition,     // Position to draw the head
                    1f,               // Alpha (opacity)
                    0.8f,             // Scale
                    headBordersColor  // Border color
                );
            }
        }
    }
}
