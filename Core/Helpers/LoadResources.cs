using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace DPSPanel.Core.Helpers
{
    internal class LoadResources
    {

        public static Asset<Texture2D> KingSlimeButton;
        public static Asset<Texture2D> KingSlimeButtonHighlight;

        public static Asset<Texture2D> SliderEmpty;
        public static Asset<Texture2D> SliderFull;

        public static Asset<Texture2D> SliderGenericEmpty;
        public static Asset<Texture2D> SliderGenericFull;

        public static Asset<Texture2D> RequestResource(string path, bool immediate = false) => ModContent.Request<Texture2D>("DPSPanel/Core/Resources/" + path, immediate ? AssetRequestMode.ImmediateLoad : AssetRequestMode.AsyncLoad);
        private static Asset<Texture2D> PreloadResource(string path) => RequestResource(path, true);

        public static void PreloadResources()
        {

            KingSlimeButton = PreloadResource("KingSlime");
            KingSlimeButtonHighlight = PreloadResource("KingSlimeHighlight");

            SliderEmpty = PreloadResource("SliderEmpty");
            SliderFull = PreloadResource("SliderFull");

            SliderGenericEmpty = PreloadResource("SliderGenericEmpty");
            SliderGenericFull = PreloadResource("SliderGenericFull");
        }
    }
}
