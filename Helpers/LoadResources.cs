using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;

namespace DPSPanel.Helpers
{
    public static class LoadAssets
    {
        public static Asset<Texture2D> BarEmpty300;
        public static Asset<Texture2D> BarFull300;
        public static Asset<Texture2D> BarEmpty150;
        public static Asset<Texture2D> BarFull150;

        public static Asset<Texture2D> ToggleButtonTexture;
        public static Asset<Texture2D> ToggleButtonTextureHighlighted;

        public static Asset<Texture2D> RequestAsset(string path, bool immediate = false) => ModContent.Request<Texture2D>("DPSPanel/Assets/" + path, immediate ? AssetRequestMode.ImmediateLoad : AssetRequestMode.AsyncLoad);
        private static Asset<Texture2D> PreloadAsset(string path) => RequestAsset(path, true);

        public static void PreloadAllAssets()
        {

            BarEmpty300 = PreloadAsset("BarEmpty300");
            BarFull300 = PreloadAsset("BarFull300");
            BarEmpty150 = PreloadAsset("BarEmpty150");
            BarFull150 = PreloadAsset("BarFull150");

            ToggleButtonTexture = PreloadAsset("ToggleButton");
            ToggleButtonTextureHighlighted = PreloadAsset("ToggleButtonHighlighted");


        }
    }
}
