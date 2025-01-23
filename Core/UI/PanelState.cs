using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace DPSPanel.Core.Panel
{
    public class PanelState : UIState
    {
        public BossPanelContainer container;

        public PanelState()
        {
            container = new BossPanelContainer();
            Append(container);
        }
    }
}
