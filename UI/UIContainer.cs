using System.Linq;
using Terraria.UI;

namespace BetterDPS.UI
{
    /// <summary>
    /// Represents a container for UI elements in Terraria's modding framework.
    /// </summary>
    /// <remarks>
    /// The UIState serves as a canvas where multiple UI elements (e.g., panels, buttons, labels) can be added and managed.
    /// It handles the organization and updating of all child elements and is used in conjunction with the UISystem to render
    /// custom interfaces in the game.
    /// </remarks>
    public class UIContainer : UIState
    {
        // Variables
        private DraggableUIPanel dpsPanel;
        private ExampleUIPanel examplePanel;

        public override void OnInitialize()
        {
            // Add the draggable panel which shows dps
            dpsPanel = new DraggableUIPanel();
            dpsPanel.Width.Set(300f, 0f);
            dpsPanel.Height.Set(200f, 0f);
            dpsPanel.Left.Set(400f, 0f);
            dpsPanel.Top.Set(200f, 0f);
            Append(dpsPanel);

            // Add the example panel
            examplePanel = new ExampleUIPanel();
            examplePanel.Width.Set(300f, 0f);
            examplePanel.Height.Set(150f, 0f);
            examplePanel.Left.Set(400f, 0f);
            examplePanel.Top.Set(200f, 0f);
            Append(examplePanel);
        }

        // Methods to toggle DPS Panel
        public void ShowDPSPanel()
        {
            if (!Children.Contains(dpsPanel))
                Append(dpsPanel);
        }

        public void HideDPSPanel()
        {
            dpsPanel.Remove();
        }

        // Methods to toggle Example Panel
        public void ShowExamplePanel()
        {
            if (!Children.Contains(examplePanel))
                Append(examplePanel);
        }

        public void HideExamplePanel()
        {
            examplePanel.Remove();
        }
    }
}
