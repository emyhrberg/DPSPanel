using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;


namespace DPSPanel.Debug.DebugMisc
{
    [Autoload(Side = ModSide.Client)]
    public class DebugSystem : ModSystem
    {
        // Variables
        private UserInterface ui;
        internal DebugState state;

        public override void OnWorldLoad()
        {
            if (!DebugConfig.IS_DEBUG_ENABLED)
            {
                return;
            }

            state = new DebugState();
            ui = new UserInterface();

            state.Activate();
            ui.SetState(state);
        }

        public override void UpdateUI(GameTime gameTime)
        {
            if (!DebugConfig.IS_DEBUG_ENABLED)
            {
                return;
            }

            ui?.Update(gameTime);
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            if (!DebugConfig.IS_DEBUG_ENABLED)
            {
                return;
            }

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
