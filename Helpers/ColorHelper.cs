using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;

namespace DPSPanel.Helpers
{
    public static class ColorHelper
    {
        public static readonly Color[] standardColors =
        [
            Color.Red,
            Color.Green,
            Color.Blue,
            Color.Yellow,
            Color.Purple,
            Color.Orange,
            Color.Cyan,
            Color.Pink,
            Color.LightGreen,
            Color.LightBlue,
            Color.LightCoral,
            Color.LightGoldenrodYellow,
        ];

        public static Color[] rainbowColors(int count = 12)
        {
            List<Color> rainbowColors = [];
            for (int i = 0; i < count; i++)
            {
                float hue = (float)i / count;
                rainbowColors.Add(Main.hslToRgb(hue, 1f, 0.5f));
            }
            return rainbowColors.ToArray();
        }
    }
}
