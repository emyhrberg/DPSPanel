using DPSPanel.Debug.DebugActions;
using Terraria.UI;

namespace DPSPanel.Debug.DebugMisc
{
    public class DebugState : UIState
    {
        public DebugPanel debugPanel;
        public DebugText debugText;

        public DebugState()
        {
            // Debug text in bottom left corner
            debugText = new DebugText();
            Append(debugText);

            debugPanel = new DebugPanel();
            Append(debugPanel);
        }
    }
}
