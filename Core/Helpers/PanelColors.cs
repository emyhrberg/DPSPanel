using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DPSPanel.Core.Helpers
{
    public static class PanelColors
    {
        public static readonly Color[] colors =
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

        public static Texture2D AddYellowOutlineToTexture(GraphicsDevice graphicsDevice, Texture2D texture, int outlineThickness)
        {
            int width = texture.Width;
            int height = texture.Height;

            // Get the original texture's pixel data
            Color[] originalData = new Color[width * height];
            texture.GetData(originalData);

            // Create a new texture for the outlined version
            Color[] outlineData = new Color[(width + outlineThickness * 2) * (height + outlineThickness * 2)];
            int outlineWidth = width + outlineThickness * 2;

            // Define the outline color as yellow
            Color outlineColor = Color.Yellow;

            // Process each pixel to detect edges
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = y * width + x;

                    // Check if this pixel is non-transparent
                    if (originalData[index].A > 0)
                    {
                        // Add outline around this pixel
                        for (int oy = -outlineThickness; oy <= outlineThickness; oy++)
                        {
                            for (int ox = -outlineThickness; ox <= outlineThickness; ox++)
                            {
                                int outlineX = x + ox + outlineThickness;
                                int outlineY = y + oy + outlineThickness;

                                // Ensure we're within bounds of the outline texture
                                if (outlineX >= 0 && outlineX < outlineWidth && outlineY >= 0 && outlineY < outlineWidth)
                                {
                                    int outlineIndex = outlineY * outlineWidth + outlineX;

                                    // Only color the outline pixel if it's currently transparent
                                    if (outlineData[outlineIndex].A == 0)
                                    {
                                        outlineData[outlineIndex] = outlineColor;
                                    }
                                }
                            }
                        }

                        // Copy the original pixel to the new texture
                        int newIndex = (y + outlineThickness) * outlineWidth + (x + outlineThickness);
                        outlineData[newIndex] = originalData[index];
                    }
                }
            }

            // Create a new texture for the outlined sprite
            Texture2D outlinedTexture = new Texture2D(graphicsDevice, outlineWidth, outlineWidth);
            outlinedTexture.SetData(outlineData);

            return outlinedTexture;
        }
    }
}
