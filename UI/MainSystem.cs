using System.Collections.Generic;
using System.Linq;
using DPSPanel.Common.Configs;
using DPSPanel.Common.DamageCalculation;
using Terraria.ID;
using Terraria.UI;
#if DEBUG
using DPSPanel.Core.Debug;
#endif

namespace DPSPanel.UI;

[Autoload(Side = ModSide.Client)]
public sealed class MainSystem : ModSystem
{
    private UserInterface ui;
    internal MainState state;
    private PanelAppearance? appearance;
    private int layoutVersion = -1;
    private Vector2 screenSize;
    private float uiScale;
    private bool rebuildRequested = true;

    public override void PostSetupContent()
    {
        DPSPanelLayout.Update();
        state = new MainState();
        ui = new UserInterface();
        state.Activate();
        ui.SetState(state);
        RequestRebuild();
    }

    public void RequestRebuild() => rebuildRequested = true;

    public override void UpdateUI(GameTime gameTime)
    {
        DPSPanelLayout.Update();
        if (state == null)
            return;
        var c = Config.Conf.C;
        var current = new PanelAppearance(c.Theme, c.Width, c.DamageDisplay,
            c.ShowPlayerIcons, c.ShowBossIcon, c.ShowTooltips);
        var screen = new Vector2(Main.screenWidth, Main.screenHeight);
        if (rebuildRequested || appearance != current || layoutVersion != DPSPanelLayout.Version ||
            screenSize != screen || uiScale != Main.UIScale)
        {
            // Config callbacks may run before the UI exists. Rebuild together on the UI thread.
            if (!state.container.dragging)
            {
                state.Recalculate();
                state.container.toggleButton.ApplyLayout();
                state.container.ClampToScreen();
                state.container.panel.RebuildAppearance();
                // A collapsed panel is detached from the container, but still needs its new size.
                state.container.Width.Set(state.container.panel.Width.Pixels, 0);
                state.container.Height.Set(state.container.panel.Height.Pixels, 0);
                state.container.ClampToScreen();
                state.container.panel.ApplyLayout();
                appearance = current;
                layoutVersion = DPSPanelLayout.Version;
                screenSize = screen;
                uiScale = Main.UIScale;
                rebuildRequested = false;
            }
        }
        if (!Main.gameMenu)
            ui?.Update(gameTime);
    }

    public void PresentEncounter(bool reset = false)
    {
        if (state == null)
            return;
#if DEBUG
        if (ModContent.GetInstance<DebugKeybinds>().PreviewActive)
            return;
#endif
        var encounter = ModContent.GetInstance<EncounterSystem>();
        if (reset)
            state.container.panel.Reset();
        state.container.panel.SetPlayers(encounter.State.VisiblePlayers().ToArray(), Main.netMode == NetmodeID.SinglePlayer);
        state.container.panel.SetFight(encounter.Current);
    }

    public void ClearDisplay()
    {
#if DEBUG
        var debug = ModContent.GetInstance<DebugKeybinds>();
        if (debug.PreviewActive)
        {
            debug.ClearPreview();
            return;
        }
#endif
        ModContent.GetInstance<EncounterSystem>().ClearVisible();
    }

    public override void Unload()
    {
        ui?.SetState(null);
        ui = null;
        state = null;
        appearance = null;
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        int index = layers.FindIndex(layer => layer.Name == "Vanilla: Mouse Text");
        if (index < 0)
            return;
        layers.Insert(index, new LegacyGameInterfaceLayer("DPSPanel: MainSystem", () =>
        {
            if (!Main.gameMenu)
                ui?.Draw(Main.spriteBatch, new GameTime());
            return true;
        }, InterfaceScaleType.UI));
    }
}
