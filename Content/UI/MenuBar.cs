using BetterDPS.Content.UI;
using Terraria.UI;

namespace BetterDPS.Content.UI
{
    class MenuBar : UIState
    {
        public PlayButton playButton;

        public override void OnInitialize()
        {
            playButton = new PlayButton();

            Append(playButton);
        }
    }
}