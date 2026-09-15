using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria.ModLoader;

namespace DPSPanel.Core.Utilities;

public class EffectLoader : ModSystem
{
    private const string GrayscalePath = "DPSPanel/Assets/Effects/Grayscale";

    // Keep Asset wrappers: tModLoader hot reload replaces their Value in place.
    private static Asset<Effect> grayscaleEffect;
    private static bool grayscaleFailed;

    public static bool TryGetGrayscaleEffect(out Effect effect)
    {
        if (grayscaleFailed)
        {
            effect = null;
            return false;
        }

        try
        {
            grayscaleEffect ??= ModContent.Request<Effect>(GrayscalePath, AssetRequestMode.ImmediateLoad);
        }
        catch (Exception e)
        {
            grayscaleFailed = true;
            Log.Warn($"Failed to load grayscale effect '{GrayscalePath}': {e.Message}");
        }

        effect = grayscaleEffect?.Value;
        return effect != null;
    }
    private const string LiquidGlassPath = "DPSPanel/Assets/Effects/LiquidGlass";

    private static Asset<Effect> liquidGlassEffect;
    private static bool liquidGlassFailed;

    public static bool TryGetLiquidGlassEffect(out Effect effect)
    {
        if (liquidGlassFailed)
        {
            effect = null;
            return false;
        }

        try
        {
            liquidGlassEffect ??= ModContent.Request<Effect>(LiquidGlassPath, AssetRequestMode.ImmediateLoad);
        }
        catch (Exception e)
        {
            liquidGlassFailed = true;
            Log.Warn($"Failed to load liquid glass effect '{LiquidGlassPath}': {e.Message}");
        }

        effect = liquidGlassEffect?.Value;
        return effect != null;
    }

    private const string RadialBarPath = "ErkySSC/Assets/Effects/RadialBar";

    private static Asset<Effect> radialBarEffect;
    private static bool radialBarFailed;

    public static bool TryGetRadialBarEffect(out Effect effect)
    {
        if (radialBarFailed)
        {
            effect = null;
            return false;
        }

        try
        {
            radialBarEffect ??= ModContent.Request<Effect>(RadialBarPath, AssetRequestMode.ImmediateLoad);
        }
        catch (Exception e)
        {
            // The wheel falls back to flat quads, so this must not throw once per frame.
            radialBarFailed = true;
            Log.Warn($"Failed to load radial bar effect '{RadialBarPath}': {e.Message}");
        }

        effect = radialBarEffect?.Value;
        return effect != null;
    }

    private const string GlassButtonPath = "ErkySSC/Assets/Effects/GlassButton";

    private static Asset<Effect> glassButtonEffect;
    private static bool glassButtonFailed;

    public static bool TryGetGlassButtonEffect(out Effect effect)
    {
        if (glassButtonFailed)
        {
            effect = null;
            return false;
        }

        try
        {
            glassButtonEffect ??= ModContent.Request<Effect>(GlassButtonPath, AssetRequestMode.ImmediateLoad);
        }
        catch (Exception e)
        {
            // Buttons fall back to flat panels, so this must not throw once per frame.
            glassButtonFailed = true;
            Log.Warn($"Failed to load glass button effect '{GlassButtonPath}': {e.Message}");
        }

        effect = glassButtonEffect?.Value;
        return effect != null;
    }

    private const string PixelButtonSelectedPath = "ErkySSC/Assets/Effects/PixelButtonSelected";

    private static Asset<Effect> pixelButtonSelectedEffect;
    private static bool pixelButtonSelectedFailed;

    public static bool TryGetPixelButtonSelectedEffect(out Effect effect)
    {
        if (pixelButtonSelectedFailed)
        {
            effect = null;
            return false;
        }

        try
        {
            pixelButtonSelectedEffect ??= ModContent.Request<Effect>(PixelButtonSelectedPath, AssetRequestMode.ImmediateLoad);
        }
        catch (Exception e)
        {
            pixelButtonSelectedFailed = true;
            Log.Warn($"Failed to load selected pixel button effect '{PixelButtonSelectedPath}': {e.Message}");
        }

        effect = pixelButtonSelectedEffect?.Value;
        return effect != null;
    }

    private const string GradientSmoothPath = "DPSPanel/Assets/Effects/GradientSmooth";

    private static Asset<Effect> gradientSmoothEffect;
    private static bool gradientSmoothFailed;

    public static bool TryGetGradientSmoothEffect(out Effect effect)
    {
        if (gradientSmoothFailed)
        {
            effect = null;
            return false;
        }

        try
        {
            gradientSmoothEffect ??= ModContent.Request<Effect>(GradientSmoothPath, AssetRequestMode.ImmediateLoad);
        }
        catch (Exception e)
        {
            // The track falls back to the raw banded strip, so this must not throw once per frame.
            gradientSmoothFailed = true;
            Log.Warn($"Failed to load gradient smoothing effect '{GradientSmoothPath}': {e.Message}");
        }

        effect = gradientSmoothEffect?.Value;
        return effect != null;
    }

    public override void Unload()
    {
        grayscaleEffect = null;
        grayscaleFailed = false;
        liquidGlassEffect = null;
        liquidGlassFailed = false;
        radialBarEffect = null;
        radialBarFailed = false;
        glassButtonEffect = null;
        glassButtonFailed = false;
        pixelButtonSelectedEffect = null;
        pixelButtonSelectedFailed = false;
        gradientSmoothEffect = null;
        gradientSmoothFailed = false;
    }
}
