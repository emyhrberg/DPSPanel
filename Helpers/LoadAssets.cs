﻿using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;

namespace DPSPanel.Helpers
{
    public class LoadAssets : ModSystem
    {
        public override void Load()
        {
            Assets.PreloadAllAssets();
        }
    }

    public static class Assets
    {
        // Textures
        public static Asset<Texture2D> BarEmpty300;
        public static Asset<Texture2D> BarFull300;
        public static Asset<Texture2D> BarEmpty150;
        public static Asset<Texture2D> BarFull150;
        public static Asset<Texture2D> ToggleButton;
        public static Asset<Texture2D> ToggleButtonHighlighted;

        public static void PreloadAllAssets()
        {
            BarEmpty300 = PreloadAsset("BarEmpty300");
            BarFull300 = PreloadAsset("BarFull300");
            BarEmpty150 = PreloadAsset("BarEmpty150");
            BarFull150 = PreloadAsset("BarFull150");
            ToggleButton = PreloadAsset("ToggleButton");
            ToggleButtonHighlighted = PreloadAsset("ToggleButtonHighlighted");
        }

        /// <summary>
        /// Preloads an asset with ImmediateLoad mode in the "DPSPanel/Assets" directory.
        /// </summary>
        private static Asset<Texture2D> PreloadAsset(string path)
        {
            return ModContent.Request<Texture2D>("DPSPanel/Assets/" + path, AssetRequestMode.ImmediateLoad);
        }
    }
}
