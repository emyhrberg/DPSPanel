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
using UI;

namespace DPSPanel.UI
{
    [Autoload(Side = ModSide.Client)]
    public class PanelSystem : ModSystem
    {
        // Variables
        private UserInterface ui;
        internal PanelState state;

        public override void Load()
        {
            LoadAssets.PreloadAllAssets();
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

            // SavePlayerHeadOnEnterWorld
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
                        // nog();

                        // Main UI system drawing
                        ui.Draw(Main.spriteBatch, new GameTime());
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
            }
        }

        public void nog()
        {
            Vector2 pos = new(500, 500);
            Player ply = Main.LocalPlayer;
            FlipHeadDrawSystem.shouldFlipHeadDraw = ply.direction == -1;
            Main.MapPlayerRenderer.DrawPlayerHead(Main.Camera, ply, pos, 1f, 1.1f, Color.White);
            FlipHeadDrawSystem.shouldFlipHeadDraw = false;
        }
    }
}
