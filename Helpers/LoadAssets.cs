using System.Reflection;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;

namespace DPSPanel.Helpers
{
    /// <summary>
    /// To add a new asset, simply add a new field like:
    /// public static Asset<Texture2D> MyAsset;
    /// </summary>
    public class LoadAssets : ModSystem
    {
        public override void Load()
        {
            _ = Ass.Initialized;
        }
    }
    public static class Ass
    {
        // My textures
        public static Asset<Texture2D> Default;
        public static Asset<Texture2D> DefaultLarge;
        public static Asset<Texture2D> BarFill;
        public static Asset<Texture2D> ToggleButton;
        public static Asset<Texture2D> ToggleButtonHighlighted;

        // Block's Combo Textures
        public static Asset<Texture2D> Fancy;
        public static Asset<Texture2D> FancyLarge;
        public static Asset<Texture2D> Golden;
        public static Asset<Texture2D> GoldenLarge;
        public static Asset<Texture2D> Leaf;
        public static Asset<Texture2D> LeafLarge;
        public static Asset<Texture2D> Retro;
        public static Asset<Texture2D> RetroLarge;
        public static Asset<Texture2D> Sticks;
        public static Asset<Texture2D> SticksLarge;
        public static Asset<Texture2D> StoneGold;
        public static Asset<Texture2D> StoneGoldLarge;
        public static Asset<Texture2D> Tribute;
        public static Asset<Texture2D> TributeLarge;
        public static Asset<Texture2D> TwigLeaf;
        public static Asset<Texture2D> TwigLeafLarge;
        public static Asset<Texture2D> Valkyrie;
        public static Asset<Texture2D> ValkyrieLarge;

        // Bool for checking if assets are loaded
        public static bool Initialized { get; set; }

        // Constructor
        static Ass()
        {
            foreach (FieldInfo field in typeof(Ass).GetFields())
            {
                if (field.FieldType == typeof(Asset<Texture2D>))
                {
                    field.SetValue(null, RequestAsset(field.Name));
                }
            }
        }

        private static Asset<Texture2D> RequestAsset(string path)
        {
            return ModContent.Request<Texture2D>($"DPSPanel/Assets/" + path, AssetRequestMode.AsyncLoad);
        }
    }
}
