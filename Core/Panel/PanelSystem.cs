using DPSPanel.Core.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI;

namespace DPSPanel.Core.Panel
{
    public class PanelSystem : ModSystem
    {
        // Variables
        private readonly UserInterface ui = new();
        internal PanelState state = new();
        
        public override void Load()
        {
            // Load resources
            if (!Main.dedServ) // Only load on client
                LoadResources.PreloadResources();
        }

        public override void PostSetupContent()
        {
            // This is called after everything in the game has been loaded
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
                    "DPSPanel: UI System", // this text doesn't matter but it's used for debugging when we want to see what's being rendered
                    delegate
                    {
                        ui.Draw(Main.spriteBatch, new GameTime());
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
            }
        }
    }
}