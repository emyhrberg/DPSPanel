using Microsoft.Xna.Framework;
using Terraria.GameContent.UI.Elements;

namespace BetterDPS.UI
{
    public class ExampleUIPanel : UIPanel
    {
        public ExampleUIPanel()
        {
            Width.Set(300f, 0f);
            Height.Set(150f, 0f);
            BackgroundColor = new Color(73, 94, 171); // Light blue background
        }
    }
}
