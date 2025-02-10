using Terraria.ModLoader;
using Terraria.UI;

namespace DPSPanel.UI
{
    public class MainState : UIState
    {
        public MainContainer container;

        public MainState()
        {
            container = new MainContainer();
            Append(container);

            ModContent.GetInstance<DPSPanel>().Logger.Info("MainState initialized!");
        }
    }
}
