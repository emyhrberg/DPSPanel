using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;

namespace DPSPanel.Helpers
{
    public static class LoadAssets
    {
        public static Asset<Texture2D> BarEmpty;
        public static Asset<Texture2D> BarFull;

        public static Asset<Texture2D> NinjaTexture;
        public static Asset<Texture2D> NinjaHighlightedTexture;

        public static Asset<Texture2D> RequestAsset(string path, bool immediate = false) => ModContent.Request<Texture2D>("DPSPanel/Assets/" + path, immediate ? AssetRequestMode.ImmediateLoad : AssetRequestMode.AsyncLoad);
        private static Asset<Texture2D> PreloadAsset(string path) => RequestAsset(path, true);

        public static void PreloadAllAssets()
        {

            BarEmpty = PreloadAsset("BarEmpty");
            BarFull = PreloadAsset("BarFull");

            NinjaTexture = PreloadAsset("Ninja");
            NinjaHighlightedTexture = PreloadAsset("NinjaHighlighted");


        }
    }
}
