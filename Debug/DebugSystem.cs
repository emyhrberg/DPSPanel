using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace DPSPanel.Debug
{
    [Autoload(Side = ModSide.Client)]
    public class DebugSystem : ModSystem
    {
        // Variables
        private UserInterface ui;
        internal DebugState state;

        public override void OnWorldLoad()
        {
            state = new DebugState();
            ui = new UserInterface();

            state.Activate();
            ui.SetState(state);
        }

        public override void UpdateUI(GameTime gameTime)
        {
            ui?.Update(gameTime);
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
            if (mouseTextIndex != -1)
            {
                layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                    "DPSPanel: DebugSystem",
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
