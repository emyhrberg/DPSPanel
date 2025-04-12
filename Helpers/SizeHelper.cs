using System.Collections.Generic;
using DPSPanel.Common.Configs;
using Terraria.ModLoader;

namespace DPSPanel.UI
{
    public static class SizeHelper
    {
        // Width is straightforward, only one dictionary needed to adjust the sizes of MainPanel and MainContainer.
        public static Dictionary<string, float> WidthSizes = new()
        {
            { "Small", 150f },
            { "Medium", 300f },
            { "Large", 450f },
        };

        public static float GetWidthFromConfig()
        {
            Config c = ModContent.GetInstance<Config>();
            string widthSize = c.Width;
            float width = 150; // default
            if (SizeHelper.WidthSizes.ContainsKey(widthSize))
            {
                width = SizeHelper.WidthSizes[widthSize];
            }
            return width;
        }

        // Height is a bit more complicated, as we need to adjust:
        // - each Bar's height. Default is 40f
        public static Dictionary<string, float> HeightSizes = new()
        {
            { "Small", 30f },
            { "Medium", 40f },
            { "Large", 50f },
        };
    }
}