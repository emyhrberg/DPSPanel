using Terraria.UI;

namespace DPSPanel.Debug.DebugMisc
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
