namespace DPSPanel.UI;

/// <summary>All config values which affect an already visible panel.</summary>
public readonly record struct PanelAppearance(string Theme, string Width, string DamageDisplay,
    bool ShowPlayerIcons, bool ShowBossIcon, bool ShowTooltips);
