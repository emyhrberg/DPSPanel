using System.ComponentModel;
using DPSPanel.Core.Configs.ConfigElements;
using DPSPanel.UI;
using Terraria.ModLoader.Config;

namespace DPSPanel.Common.Configs;

public sealed class Config : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ClientSide;

    [Header("DamageCalculation")]
    [BackgroundColor(192, 54, 64)]
    [DefaultValue(true)]
    public bool TrackAllEntities = true;

    [ConfigIcon(nameof(Ass.ConfigUnknownItem))]
    [BackgroundColor(192, 54, 64)]
    [DefaultValue(true)]
    public bool TrackUnknownDamage = true;

    [Header("UI")]
    [DrawTicks]
    [CustomModConfigItem(typeof(ThemeConfigElement))]
    [OptionStrings(["Default", "Fancy", "Golden", "Leaf", "Retro", "Sticks", "StoneGold", "Tribute", "TwigLeaf", "Valkyrie"])]
    [DefaultValue("Default")]
    [BackgroundColor(255, 192, 8)]
    public string Theme = "Default";

    [DrawTicks]
    [OptionStrings(["Small", "Medium", "Large"])]
    [DefaultValue("Small")]
    [BackgroundColor(255, 192, 8)]
    public string Width = "Small";

    [Header("Settings")]
    [DrawTicks]
    [OptionStrings(["Damage", "Percent"])]
    [DefaultValue("Damage")]
    [BackgroundColor(85, 111, 64)]
    public string DamageDisplay = "Damage";

    [CustomModConfigItem(typeof(ShowPlayerIconConfigElement))]
    [BackgroundColor(85, 111, 64)]
    [DefaultValue(true)]
    public bool ShowPlayerIcons = true;

    [BackgroundColor(85, 111, 64)]
    [DefaultValue(true)]
    public bool ShowBossIcon = true;

    [BackgroundColor(85, 111, 64)]
    [DefaultValue(true)]
    public bool ShowTooltips = true;

    [BackgroundColor(85, 111, 64)]
    [DefaultValue(true)]
    public bool MakePanelDraggable = true;

    public override void OnChanged()
    {
        // tModLoader can load config before the client UI exists.
        if (Main.dedServ)
            return;
        ModContent.GetInstance<MainSystem>()?.state?.container?.panel?.ApplyLayout();
    }

    public static class Conf
    {
        public static Config C => ModContent.GetInstance<Config>();
    }
}
