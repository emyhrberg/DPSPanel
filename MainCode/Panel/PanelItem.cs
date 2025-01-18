using Microsoft.Xna.Framework;
using Terraria.UI;
using Terraria.GameContent.UI.Elements;

namespace DPSPanel.MainCode.Panel
{
    public class PanelItem : UIElement
    {
        private int _value;
        private UITextPanel<string> _textPanel;

        public PanelItem(int value, UITextPanel<string> textPanel)
        {
            _value = value;
            _textPanel = textPanel;
        }
    }
}
