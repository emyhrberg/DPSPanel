using Terraria.UI;

namespace DPSPanel.Debug
{
    public class DebugState : UIState
    {
        public DebugPanel debugPanel;

        public DebugState()
        {
            debugPanel = new DebugPanel();
            Append(debugPanel);
        }
    }
}
