using Terraria.UI;

namespace BetterDPS.Content.UI
{
    /* This class is an example of a UIState that contains a draggable panel. */
    public class ExampleUIState : UIState
    {
        private DraggableUIPanel panel;

        public override void OnInitialize()
        {
            panel = new DraggableUIPanel();
            panel.Width.Set(300f, 0f);
            panel.Height.Set(200f, 0f);
            panel.Left.Set(400f, 0f);
            panel.Top.Set(200f, 0f);

            Append(panel);
        }
    }
}
