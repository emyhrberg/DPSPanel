using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;

namespace DPSPanel.Helpers
{
    public static class LoadResources
    {
        public static Asset<Texture2D> BarEmpty;
        public static Asset<Texture2D> BarFull;

        public static Asset<Texture2D> NinjaTexture;
        public static Asset<Texture2D> NinjaHighlightedTexture;

        public static Asset<Texture2D> RequestResource(string path, bool immediate = false) => ModContent.Request<Texture2D>("DPSPanel/Assets/" + path, immediate ? AssetRequestMode.ImmediateLoad : AssetRequestMode.AsyncLoad);
        private static Asset<Texture2D> PreloadResource(string path) => RequestResource(path, true);

        public static void PreloadResources()
        {

            BarEmpty = PreloadResource("BarEmpty");
            BarFull = PreloadResource("BarFull");

            NinjaTexture = PreloadResource("Ninja");
            NinjaHighlightedTexture = PreloadResource("NinjaHighlighted");


        }
    }
}
