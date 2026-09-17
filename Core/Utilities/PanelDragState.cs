using System;

namespace DPSPanel.Core.Utilities;

/// <summary>One captured pointer gesture, entirely in UI coordinates.</summary>
public sealed class PanelDragState
{
    private float pressX;
    private float pressY;
    private float grabX;
    private float grabY;
    public bool PointerDown { get; private set; }
    public bool IsDragging { get; private set; }
    public bool SuppressClick { get; private set; }

    public void Begin(float mouseX, float mouseY, float panelX, float panelY)
    {
        pressX = mouseX;
        pressY = mouseY;
        grabX = mouseX - panelX;
        grabY = mouseY - panelY;
        PointerDown = true;
        IsDragging = SuppressClick = false;
    }

    public (float X, float Y)? Move(float mouseX, float mouseY, float threshold)
    {
        if (!PointerDown)
            return null;
        float dx = mouseX - pressX;
        float dy = mouseY - pressY;
        threshold = Math.Max(0, threshold);
        if (!IsDragging && dx * dx + dy * dy <= threshold * threshold)
            return null;
        // Latch this even if the pointer later returns to the original press position.
        IsDragging = SuppressClick = true;
        return (mouseX - grabX, mouseY - grabY);
    }

    public void End(bool cancelled = false)
    {
        SuppressClick |= cancelled;
        PointerDown = IsDragging = false;
    }
}
