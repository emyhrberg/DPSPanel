using System.Collections.Generic;
using System.Linq;
using DPSPanel.Common.Configs;
using DPSPanel.Common.DamageCalculation;
using Terraria.GameInput;
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
    private GameTime lastGameTime = new();

    // UserInterface.Update already reads the UI-scaled mouse supplied by Terraria.
    // Use our own interface's sample; ActiveInstance may belong to another mod between frames.
    internal static Vector2 PointerPosition => ModContent.GetInstance<MainSystem>()?.ui?.MousePosition ?? Main.MouseScreen;
    internal static Vector2 ViewportSize => PlayerInput.OriginalScreenSize / System.Math.Max(0.1f, Main.UIScale);

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
        lastGameTime = gameTime;
        DPSPanelLayout.Update();
        if (state == null)
            return;
        var c = Config.Conf.C;
        var current = new PanelAppearance(c.Theme, c.Width, c.DamageDisplay,
            c.ShowPlayerIcons, c.ShowBossIcon, c.ShowTooltips);
        var screen = PlayerInput.OriginalScreenSize;
        bool viewportChanged = screenSize != screen || uiScale != Main.UIScale;
        if (viewportChanged || Main.gameMenu || !Main.hasFocus)
            state.container.CancelPointer();
        // Main calls UpdateUI after PlayerInput.SetZoom_UI: input is already in UI pixels.
        // Select this interface before recalculation/hit testing, just as Draw does.
        ui.Use();
        if (rebuildRequested || appearance != current || layoutVersion != DPSPanelLayout.Version ||
            viewportChanged)
        {
            // Config callbacks may run before the UI exists. Rebuild together on the UI thread.
            if (!state.container.IsPointerCaptured)
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

    public override void OnWorldUnload()
    {
        state?.container.CancelPointer();
        ui?.EscapeElements();
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        int index = layers.FindIndex(layer => layer.Name == "Vanilla: Mouse Text");
        if (index < 0)
            return;
        layers.Insert(index, new LegacyGameInterfaceLayer("DPSPanel: MainSystem", () =>
        {
            if (!Main.gameMenu)
                // The UI layer already begins the batch with Main.UIScaleMatrix.
                ui?.Draw(Main.spriteBatch, lastGameTime);
            return true;
        }, InterfaceScaleType.UI));
    }
}
