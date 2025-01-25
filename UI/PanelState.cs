using DPSPanel.Helpers;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;
using Terraria.UI;

namespace DPSPanel.UI

{
    public class PanelState : UIState
    {
        public BossContainerElement container;

        public PanelState()
        {
            container = new BossContainerElement();
            Append(container);

            ModContent.GetInstance<DPSPanel>().Logger.Info("PanelState initialized!");
        }
    }
}
