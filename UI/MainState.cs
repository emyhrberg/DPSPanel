using Terraria.UI;

namespace DPSPanel.UI;

public sealed class MainState : UIState
{
    public MainContainer container;
    public PlayerDamagePanel Popup { get; }

    public MainState()
    {
        container = new MainContainer();
        Append(container);
        Popup = new PlayerDamagePanel();
        Append(Popup);
        container.panel.Popup = Popup;
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        container.panel.UpdateHover(Main.MouseScreen / Main.UIScale);
    }
}
