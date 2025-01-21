using Terraria;
using Terraria.ID;
using Terraria.UI;

namespace DPSPanel.Core.Panel
{
    public class PanelState : UIState
    {

        public BossPanelContainer container;

        // state variables
        // public NPC currentBoss;

        public PanelState()
        {
            container = new BossPanelContainer();
            Append(container);
        }

        public void UpdateBoss(NPC boss)
        {
            // currentBoss = boss;
            container.panel.SetBossTitle(boss.FullName, boss);
            container.bossIcon.UpdateBossIcon(boss);
        }
    }
}
