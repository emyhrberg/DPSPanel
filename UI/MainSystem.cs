using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace DPSPanel.UI
{
    [Autoload(Side = ModSide.Client)]
    public class MainSystem : ModSystem
    {
        // Variables
        private UserInterface ui;
        internal MainState state;

        public override void PostSetupContent()
        {
            state = new MainState();
            ui = new UserInterface();

            state.Activate();
            ui.SetState(state);
            // Log.Info("MainSystem initialized!");
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
                    "DPSPanel: MainSystem",
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
