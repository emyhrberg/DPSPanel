using System;

namespace DPSPanel.UI;

/// <summary>
/// Edit the values inside Update() and apply .NET Hot Reload. MainSystem calls this method
/// every frame, detects Version changes, and rebuilds both panels after dragging ends.
/// Values are UI pixels (before Terraria's UI scale). Colors are RGBA.
/// </summary>
public static class DPSPanelLayout
{
    public static int Version { get; private set; }

    // Panel widths and overall height limits (0 = automatic height).
    public static float SmallWidth;
    public static float MediumWidth;
    public static float LargeWidth;
    public static float MainMinHeight;
    public static float MainMaxHeight;
    public static float PopupWidthMultiplier;
    public static float PopupMinHeight;
    public static float PopupMaxHeight;
    public static float ScreenMargin;

    // Main panel padding and row area.
    public static float PanelPaddingLeft;
    public static float PanelPaddingRight;
    public static float PanelPaddingTop;
    public static float PanelPaddingBottom;
    public static float HeaderHeight;
    public static float HeaderGap;
    public static float RowInsetLeft;
    public static float RowInsetRight;
    public static float PlayerBarHeight;
    public static float WeaponBarHeight;
    public static float PlayerRowGap;
    public static float WeaponRowGap;

    // Weapon popup.
    public static float PopupPaddingLeft;
    public static float PopupPaddingRight;
    public static float PopupPaddingTop;
    public static float PopupPaddingBottom;
    public static float PopupGap;
    public static float PopupOffsetY;

    // Header title and boss icon.
    public static float HeaderTextScale;
    public static float HeaderTextOffsetX;
    public static float HeaderTextOffsetY;
    public static float HeaderTextPaddingRight;
    public static float HeaderContentHeight;
    public static float HeaderToggleGap;
    public static float BossIconSize;
    public static float BossIconOffsetX;
    public static float BossIconOffsetY;
    public static float BossIconTextGap;

    // Bar text and icons.
    public static float BarTextScale;
    public static float BarTextOffsetX;
    public static float BarTextOffsetY;
    public static float BarTextPaddingLeft;
    public static float BarTextPaddingRight;
    public static float IconTextGap;
    public static float WeaponIconSize;
    public static float WeaponIconLeft;
    public static float WeaponIconOffsetY;
    public static float PlayerIconSize;
    public static float PlayerIconLeft;
    public static float PlayerIconOffsetY;
    public static float PlayerHeadScale;

    // Toggle button and scrolling.
    public static float ToggleWidth;
    public static float ToggleHeight;
    public static float ToggleLeft;
    public static float ToggleTop;
    public static float ToggleIconScale;
    public static float DragThreshold;
    public static float ScrollStep;
    public static float ScrollbarWidth;
    public static float ScrollbarGap;
    public static float ScrollbarMinThumbHeight;

    public static LayoutColor PanelBackground;
    public static LayoutColor PopupBackground;
    public static LayoutColor PanelBorder;
    public static LayoutColor BarOutline;
    public static LayoutColor TextColor;
    public static LayoutColor IconColor;
    public static LayoutColor ScrollbarColor;

    static DPSPanelLayout() => Update();

    public static bool Update()
    {
        bool changed = false;

        // Panel widths and overall height limits (0 = automatic height).
        Set(ref SmallWidth, 150f, ref changed);
        Set(ref MediumWidth, 300f, ref changed);
        Set(ref LargeWidth, 450f, ref changed);
        Set(ref MainMinHeight, 0f, ref changed);
        Set(ref MainMaxHeight, 0f, ref changed);
        Set(ref PopupWidthMultiplier, 1f, ref changed);
        Set(ref PopupMinHeight, 0f, ref changed);
        Set(ref PopupMaxHeight, 0f, ref changed);
        Set(ref ScreenMargin, 8f, ref changed);

        // Main panel padding and row area.
        Set(ref PanelPaddingLeft, 5f, ref changed);
        Set(ref PanelPaddingRight, 5f, ref changed);
        Set(ref PanelPaddingTop, 5f, ref changed);
        Set(ref PanelPaddingBottom, 5f, ref changed);
        Set(ref HeaderHeight, 40f, ref changed);
        Set(ref HeaderGap, 0f, ref changed);
        Set(ref RowInsetLeft, 0f, ref changed);
        Set(ref RowInsetRight, 0f, ref changed);
        Set(ref PlayerBarHeight, 40f, ref changed);
        Set(ref WeaponBarHeight, 40f, ref changed);
        Set(ref PlayerRowGap, 10f, ref changed);
        Set(ref WeaponRowGap, 10f, ref changed);

        // Weapon popup.
        Set(ref PopupPaddingLeft, 5f, ref changed);
        Set(ref PopupPaddingRight, 5f, ref changed);
        Set(ref PopupPaddingTop, 5f, ref changed);
        Set(ref PopupPaddingBottom, 5f, ref changed);
        Set(ref PopupGap, 4f, ref changed);
        Set(ref PopupOffsetY, 0f, ref changed);

        // Header title and boss icon.
        Set(ref HeaderTextScale, 0.9f, ref changed);
        Set(ref HeaderTextOffsetX, 0f, ref changed);
        Set(ref HeaderTextOffsetY, 0f, ref changed);
        Set(ref HeaderTextPaddingRight, 2f, ref changed);
        Set(ref HeaderContentHeight, 30f, ref changed);
        Set(ref HeaderToggleGap, 4f, ref changed);
        Set(ref BossIconSize, 26f, ref changed);
        Set(ref BossIconOffsetX, 0f, ref changed);
        Set(ref BossIconOffsetY, 2f, ref changed);
        Set(ref BossIconTextGap, 2f, ref changed);

        // Bar text and icons.
        Set(ref BarTextScale, 0.8f, ref changed);
        Set(ref BarTextOffsetX, 0f, ref changed);
        Set(ref BarTextOffsetY, 0f, ref changed);
        Set(ref BarTextPaddingLeft, 7f, ref changed);
        Set(ref BarTextPaddingRight, 7f, ref changed);
        Set(ref IconTextGap, 3f, ref changed);
        Set(ref WeaponIconSize, 30f, ref changed);
        Set(ref WeaponIconLeft, 5f, ref changed);
        Set(ref WeaponIconOffsetY, 0f, ref changed);
        Set(ref PlayerIconSize, 30f, ref changed);
        Set(ref PlayerIconLeft, 4f, ref changed);
        Set(ref PlayerIconOffsetY, 0f, ref changed);
        Set(ref PlayerHeadScale, 0.8f, ref changed);

        // Toggle button and scrolling.
        Set(ref ToggleWidth, 30f, ref changed);
        Set(ref ToggleHeight, 30f, ref changed);
        Set(ref ToggleLeft, 4f, ref changed);
        Set(ref ToggleTop, 4f, ref changed);
        Set(ref ToggleIconScale, 0.8f, ref changed);
        Set(ref DragThreshold, 5f, ref changed);
        Set(ref ScrollStep, 40f, ref changed);
        Set(ref ScrollbarWidth, 4f, ref changed);
        Set(ref ScrollbarGap, 3f, ref changed);
        Set(ref ScrollbarMinThumbHeight, 12f, ref changed);

        Set(ref PanelBackground, new(49, 84, 141, 255), ref changed);
        Set(ref PopupBackground, new(27, 29, 85, 255), ref changed);
        Set(ref PanelBorder, new(0, 0, 0, 255), ref changed);
        Set(ref BarOutline, new(169, 169, 169, 255), ref changed);
        Set(ref TextColor, new(255, 255, 255, 255), ref changed);
        Set(ref IconColor, new(255, 255, 255, 255), ref changed);
        Set(ref ScrollbarColor, new(180, 195, 230, 200), ref changed);

        if (changed)
            Version++;
        return changed;
    }

    public static float WidthFor(string width) => Math.Max(50f, width switch
    {
        "Medium" => MediumWidth,
        "Large" => LargeWidth,
        _ => SmallWidth
    });

    private static void Set(ref float target, float value, ref bool changed)
    {
        if (!float.IsFinite(value) || Math.Abs(target - value) <= 0.001f)
            return;
        target = value;
        changed = true;
    }

    private static void Set(ref LayoutColor target, LayoutColor value, ref bool changed)
    {
        if (target == value)
            return;
        target = value;
        changed = true;
    }
}

public readonly record struct LayoutColor(byte R, byte G, byte B, byte A);
