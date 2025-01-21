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

        public static Asset<Texture2D> BarFancyEmpty;
        public static Asset<Texture2D> BarFancyFull;

        public static Asset<Texture2D> BarGenericEmpty;
        public static Asset<Texture2D> BarGenericFull;

        public static Asset<Texture2D> CoolDown;

        public static Asset<Texture2D> RequestResource(string path, bool immediate = false) => ModContent.Request<Texture2D>("DPSPanel/Core/Resources/" + path, immediate ? AssetRequestMode.ImmediateLoad : AssetRequestMode.AsyncLoad);
        private static Asset<Texture2D> PreloadResource(string path) => RequestResource(path, true);

        public static void PreloadResources()
        {

            BarFancyEmpty = PreloadResource("BarFancyEmpty");
            BarFancyFull = PreloadResource("BarFancyFull");

            BarGenericEmpty = PreloadResource("BarGenericEmpty");
            BarGenericFull = PreloadResource("BarGenericFull");

            CoolDown = PreloadResource("CoolDown");
        }
    }
}
