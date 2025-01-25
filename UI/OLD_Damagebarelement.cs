// using Microsoft.Xna.Framework;
// using Microsoft.Xna.Framework.Graphics;
// using ReLogic.Content;
// using Terraria.UI;
// using Terraria.GameContent.UI.Elements;
// using Terraria;
// using Terraria.GameContent;
// using Terraria.ID;
// using Terraria.ModLoader;
// using DPSPanel.Helpers;
// using DPSPanel.Configs;

// namespace DPSPanel.UI
// {
//     public class DamageBarElement : UIElement
//     {
//         private readonly Asset<Texture2D> emptyBar; // Background 
//         private readonly Asset<Texture2D> fullBar;  // Foreground fill texture
//         private readonly UIText textElement;        // Text element for displaying player info
//         private const float ItemHeight = 40f;       // Height of each damage bar

//         private Color fillColor;     // Color for the fill
//         private int percentage;      // Progress percentage (0-100)

//         // Weapon icon
//         private int weaponItemID;
//         private string weaponName;   // Weapon name

//         // Properties
//         public string PlayerName { get; set; }
//         public int PlayerDamage { get; set; }
//         public int PlayerWhoAmI { get; set; }

//         // Player head element
//         private PlayerHeadElement playerHeadElement;

//         public DamageBarElement(float currentYOffset, string playerName, int playerWhoAmI)
//         {
//             // Load bar textures
//             emptyBar = LoadAssets.BarEmpty;
//             fullBar = LoadAssets.BarFull;

//             // Set dimensions and alignment
//             Width = new StyleDimension(0, 1.0f); // Fill the width of the parent
//             Height = new StyleDimension(ItemHeight, 0f); // Set fixed height
//             Top = new StyleDimension(currentYOffset, 0f);
//             HAlign = 0.5f; // Center horizontally

//             // Initialize player properties
//             PlayerName = playerName;
//             PlayerDamage = 0;
//             PlayerWhoAmI = playerWhoAmI;

//             // Create and center the text element
//             textElement = new UIText("", 0.8f) // 80% scale
//             {
//                 HAlign = 0.5f,
//                 VAlign = 0.5f,
//             };
//             Append(textElement);

//             // Add player head if the player is active
//             if (playerWhoAmI >= 0 && playerWhoAmI < Main.player.Length && Main.player[playerWhoAmI].active)
//             {
//                 Player player = Main.player[playerWhoAmI];
//                 playerHeadElement = new PlayerHeadElement(player, 32, new Vector2(0, 0));
//                 playerHeadElement.Left.Set(0f, 0.0f); // No offset, centered by alignment
//                 playerHeadElement.Top.Set(0f, 0.0f);  // No offset, centered by alignment
//                 Append(playerHeadElement);
//             }
//         }

//         public void UpdateDamageBar(int percentage, string playerName, int playerDamage, Color fillColor)
//         {
//             this.percentage = percentage;
//             this.fillColor = fillColor;

//             PlayerName = playerName;
//             PlayerDamage = playerDamage;

//             textElement.SetText($"{playerName} ({playerDamage})");
//         }

//         protected override void DrawSelf(SpriteBatch spriteBatch)
//         {
//             base.DrawSelf(spriteBatch);
//             DrawDamageBarFill(spriteBatch);
//             DrawDamageBarOutline(spriteBatch);
//             // Uncomment if you want to draw the weapon icon
//             // DrawWeaponIcon(spriteBatch);
//         }

//         private void DrawDamageBarFill(SpriteBatch spriteBatch)
//         {
//             CalculatedStyle dims = GetDimensions();
//             Vector2 position = new Vector2(dims.X, dims.Y);

//             // Calculate the width of the filled portion based on percentage
//             int fillWidth = (int)(dims.Width * (percentage / 100f));
//             if (fillWidth > 0)
//             {
//                 Rectangle sourceRect = new Rectangle(0, 0, (int)(fullBar.Width() * (percentage / 100f)), fullBar.Height());
//                 Rectangle destRect = new Rectangle((int)position.X, (int)position.Y, fillWidth, (int)dims.Height);

//                 spriteBatch.Draw(fullBar.Value, destRect, sourceRect, fillColor);
//             }
//         }

//         private void DrawDamageBarOutline(SpriteBatch spriteBatch)
//         {
//             CalculatedStyle dims = GetDimensions();
//             Vector2 position = new Vector2(dims.X, dims.Y);
//             Rectangle rect = new Rectangle((int)position.X, (int)position.Y, (int)dims.Width, (int)dims.Height);
//             spriteBatch.Draw(emptyBar.Value, rect, Color.DarkGray);
//         }

//         /// <summary>
//         /// Draws a semi-transparent green rectangle around the DamageBarElement for debugging.
//         /// </summary>
//         private void DrawDebugRectangle(SpriteBatch spriteBatch)
//         {
//             CalculatedStyle dims = GetDimensions();
//             Rectangle damageBarRect = new Rectangle((int)dims.X, (int)dims.Y, (int)dims.Width, (int)dims.Height);

//             // Draw the rectangle borders
//             DrawRectangle(spriteBatch, damageBarRect, Color.Green * 0.5f);
//         }

//         /// <summary>
//         /// Utility method to draw a semi-transparent rectangle.
//         /// </summary>
//         private void DrawRectangle(SpriteBatch spriteBatch, Rectangle rect, Color color)
//         {
//             // Use TextureAssets.MagicPixel.Value as the 1x1 white texture
//             Texture2D magicPixel = TextureAssets.MagicPixel.Value;

//             // Draw the rectangle borders
//             // Top
//             spriteBatch.Draw(magicPixel, new Rectangle(rect.X, rect.Y, rect.Width, 1), color);
//             // Bottom
//             spriteBatch.Draw(magicPixel, new Rectangle(rect.X, rect.Y + rect.Height - 1, rect.Width, 1), color);
//             // Left
//             spriteBatch.Draw(magicPixel, new Rectangle(rect.X, rect.Y, 1, rect.Height), color);
//             // Right
//             spriteBatch.Draw(magicPixel, new Rectangle(rect.X + rect.Width - 1, rect.Y, 1, rect.Height), color);
//         }

//         /// <summary>
//         /// Draws the weapon icon (optional).
//         /// </summary>
//         private void DrawWeaponIcon(SpriteBatch spriteBatch)
//         {
//             // Check if the weapon icon display is enabled in the config
//             // Config c = ModContent.GetInstance<Config>();
//             // if (!c.AlwaysShowButton)
//             //     return;

//             // Load the appropriate texture based on the weaponItemID
//             Texture2D texture;
//             if (weaponItemID == -1) // Invalid item ID, use a default or placeholder icon
//                 texture = TextureAssets.Buff[BuffID.Confused].Value; // Example: Confused debuff as placeholder
//             else
//                 texture = TextureAssets.Item[weaponItemID].Value;

//             // Get the dimensions of the current UI element
//             CalculatedStyle dims = GetDimensions();

//             // Define the desired maximum icon height
//             const int maxIconHeight = 32;
//             const int paddingLeft = 5; // Padding from the left edge

//             // Get original texture size
//             int originalWidth = texture.Width;
//             int originalHeight = texture.Height;

//             float scale;

//             // Determine scaling factor based on original height
//             if (originalHeight > maxIconHeight)
//             {
//                 // Scale down to have a height of 32 pixels
//                 scale = (float)maxIconHeight / originalHeight;
//             }
//             else
//             {
//                 // Use original size (no scaling)
//                 scale = 1f;
//             }

//             // Calculate scaled width and height while maintaining aspect ratio
//             int scaledWidth = (int)(originalWidth * scale);
//             int scaledHeight = (int)(originalHeight * scale);

//             // Calculate position:
//             // - X: padding from the left
//             // - Y: vertically centered within the DamageBarElement
//             int iconX = (int)dims.X + paddingLeft;
//             int iconY = (int)(dims.Y + (dims.Height - scaledHeight) / 2f);

//             // Define the destination rectangle with the calculated size and position
//             Rectangle destRect = new Rectangle(iconX, iconY, scaledWidth, scaledHeight);

//             // Draw the texture scaled to fit the destination rectangle
//             spriteBatch.Draw(texture, destRect, Color.White);
//         }
//     }
// }
