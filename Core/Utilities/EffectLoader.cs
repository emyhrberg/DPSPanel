using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria.ModLoader;

namespace DPSPanel.Core.Utilities;

[Autoload(Side = ModSide.Client)]
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

    public override void Unload()
    {
        grayscaleEffect = null;
        grayscaleFailed = false;
    }
}
