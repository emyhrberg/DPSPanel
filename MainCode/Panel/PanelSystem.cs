using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace DPSPanel.MainCode.Panel
{
    public class PanelSystem : ModSystem
    {
        // Variables
        private UserInterface ui = new();
        internal PanelState state = new();

        public override void PostSetupContent()
        {
            state.Activate();
            ui.SetState(state);
        }

        public override void UpdateUI(GameTime gameTime)
        {
            // always update the UI (everything in the PanelState, Panel, etc.)
            ui?.Update(gameTime);
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            // insert the UIContainer into the interface layers and render it
            // then we insert it before the mouse text layer because we want it to be rendered above the mouse text
            // also we check if the mouse text layer exists because it might not exist if the player has disabled the UI
            // Note: Kinda boilerplate code, but it's necessary to ensure the UI is rendered correctly
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
