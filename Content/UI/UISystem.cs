using BetterDPS.Content.UI;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace BetterDPS.Content.UI
{
    public class ExampleUISystem : ModSystem
    {
        private UserInterface exampleInterface;
        internal ExampleUIState exampleUIState;

        public override void Load()
        {
            exampleUIState = new ExampleUIState();
            exampleUIState.Activate();
            exampleInterface = new UserInterface();
            exampleInterface.SetState(exampleUIState);
        }

        public override void UpdateUI(GameTime gameTime)
        {
            exampleInterface?.Update(gameTime);
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
            if (mouseTextIndex != -1)
            {
                layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                    "ExampleMod: Example UI",
                    delegate {
                        exampleInterface.Draw(Main.spriteBatch, new GameTime());
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
            }
        }
    }
}
