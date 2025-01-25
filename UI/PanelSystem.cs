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
                        // Main drawing function
                        // DrawAllPlayerHeads();

                        // Main UI system drawing
                        ui.Draw(Main.spriteBatch, new GameTime());
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
            }
        }

        private void DrawAllPlayerHeads()
        {
            if (playerRendersField == null || playerRenders == null)
                return;

            int index = 0;

            foreach (Player player in Main.player)
            {
                if (player == null || !player.active)
                    continue;

                if (player.whoAmI < 0 || player.whoAmI >= playerRenders.Length)
                {
                    ModContent.GetInstance<DPSPanel>().Logger.Warn($"Player {player.name} has invalid whoAmI index {player.whoAmI}.");
                    continue;
                }

                PlayerHeadDrawRenderTargetContent playerHeadRender = playerRenders[player.whoAmI];
                if (playerHeadRender == null)
                {
                    ModContent.GetInstance<DPSPanel>().Logger.Warn($"PlayerHeadDrawRenderTargetContent for player {player.name} is null.");
                    continue;
                }

                // Backup and force the player to face right
                int originalDirection = player.direction;
                player.direction = 1;

                // Setup render target
                playerHeadRender.UsePlayer(player);
                playerHeadRender.UseColor(Main.GetPlayerHeadBordersColor(player));
                playerHeadRender.Request();

                // Check if the render target is ready
                if (playerHeadRender.IsReady)
                {
                    RenderTarget2D target = playerHeadRender.GetTarget();

                    // Define the destination rectangle
                    Rectangle destinationRect = new Rectangle(
                        (int)(startPosition.X + index * spacing),
                        (int)(startPosition.Y),
                        32, // Width of the head
                        32  // Height of the head
                    );

                    // Draw the player head with SpriteEffects.FlipHorizontally to ensure it faces right
                    Main.spriteBatch.Draw(
                        target,
                        destinationRect,
                        null,
                        Color.White,
                        0f,
                        target.Size() / 2f,
                        SpriteEffects.None, // No flipping needed since direction is set to 1
                        0f
                    );

                    // Restore the player's original direction
                    player.direction = originalDirection;

                    // Debug info
                    // ModContent.GetInstance<DPSPanel>().Logger.Info($"Drawing Player {player.name}'s head at {destinationRect.Location}.");
                }
                else
                {
                    ModContent.GetInstance<DPSPanel>().Logger.Info($"Player {player.name}'s head render target is not ready.");
                }

                index++;
            }
        }
    }
}
