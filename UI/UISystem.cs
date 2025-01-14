using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace BetterDPS.UI
{
    /// <summary>
    /// Manages the lifecycle and integration of a UIState with Terraria's interface layers.
    /// </summary>
    /// <remarks>
    /// The UISystem is responsible for initializing, updating, and toggling the visibility of a custom UIState.
    /// It ensures the UI is rendered correctly by injecting it into Terraria's interface layers via ModifyInterfaceLayers.
    /// This class is also used to manage whether the UIState is currently active and visible.
    /// </remarks>
    public class UISystem : ModSystem
    {
        // Variables
        private UserInterface ui;
        internal UIContainer container;

        public override void Load()
        {
            // initialization code for the UI system
            container = new UIContainer();
            container.Activate();
            ui = new UserInterface();
            ui.SetState(container);
        }

        public override void UpdateUI(GameTime gameTime)
        {
            // always update the UI (everything in the UIContainer)
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
                    "BetterDPS: UI System",
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
