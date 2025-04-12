using DPSPanel.Common.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader.Config.UI;
using Terraria.UI;
using Terraria.UI.Chat;

namespace DPSPanel.Common.Configs
{
    /// <summary>
    /// Reference:
    /// <see cref="Terraria.ModLoader.Config.UI.BooleanElement"/> 
    /// </summary>
    public class ShowPlayerIconConfigElement : ConfigElement<bool>
    {
        private Asset<Texture2D> _toggleTexture = Asset<Texture2D>.Empty;

        // Called once when the config UI binds this element to your Width property
        public override void OnBind()
        {
            base.OnBind();

            _toggleTexture = Main.Assets.Request<Texture2D>("Images/UI/Settings_Toggle");

            // Toggle the value when clicked
            OnLeftClick += delegate
            {
                Value = !Value;
            };
        }

        public override void OnInitialize()
        {
            base.OnInitialize();
        }

        public override void Draw(SpriteBatch sb)
        {
            base.Draw(sb);
            DrawPlayerIcon(sb);
            DrawToggleTexture(sb);
            DrawOnOffText(sb);
        }

        private void DrawPlayerIcon(SpriteBatch sb)
        {
            Player player = Main.LocalPlayer;

            CalculatedStyle dims = GetDimensions();
            int xOffset = 16;
            int yOffset = 14;
            Vector2 drawPosition = new(dims.X + xOffset, dims.Y + yOffset);

            // assign the shouldFlipHeadDraw flag to the direction of the player,
            // meaning the head will be flipped if the player is facing left
            PlayerHeadFlipSystem.shouldFlipHeadDraw = player.direction == -1;

            Color color = Color.White * 1.0f;
            if (Value == false)
            {
                color = Color.Red;
            }

            Main.MapPlayerRenderer.DrawPlayerHead(
                Main.Camera,
                player, // player to draw
                drawPosition,
                1f, // alpha/transparency
                0.6f, // scale
                color // border color
            );
            PlayerHeadFlipSystem.shouldFlipHeadDraw = false;
        }

        private void DrawToggleTexture(SpriteBatch sb)
        {
            // Draw toggle texture
            Rectangle sourceRectangle = new Rectangle(
                Value ? ((_toggleTexture.Width() - 2) / 2 + 2) : 0,
                0,
                (_toggleTexture.Width() - 2) / 2,
                _toggleTexture.Height()
            );

            // Get the dimensions of the parent element
            CalculatedStyle dimensions = GetDimensions();

            // Calculate the position to draw the toggle texture
            sb.Draw(
                _toggleTexture.Value,
                new Vector2(
                    dimensions.X + dimensions.Width - sourceRectangle.Width - 10f,
                    dimensions.Y + 8f
                ),
                sourceRectangle,
                Color.White
            );
        }

        private void DrawOnOffText(SpriteBatch sb)
        {
            CalculatedStyle dimensions = GetDimensions();

            string label = Value ? "On" : "Off";

            // Shift to a position you know is visible
            Vector2 pos = new Vector2(dimensions.X + dimensions.Width - 60f, dimensions.Y + 8f);
            ChatManager.DrawColorCodedStringWithShadow(
                sb,
                FontAssets.ItemStack.Value,
                label,
                pos,
                Color.White,
                0f,
                Vector2.Zero,
                new Vector2(0.8f)
            );
        }
    }
}

// Called every frame while the in-game config UI is open
// public override void Update(GameTime gameTime)
// {
//     base.Update(gameTime);

//     // If you want the tooltip to reflect changes in real-time, 
//     // update the TooltipFunction (or an internal field used by GetTooltip/TooltipFunction) here:
//     TooltipFunction = () => GetDynamicTooltip();
// }

// private string GetDynamicTooltip()
// {
//     // For example, check some global property:
//     DebugSystem debugSystem = ModContent.GetInstance<DebugSystem>();
//     int count = debugSystem.state.debugPanel.currentWeapons.Count;

//     if (count > 3)
//     {
//         return "triple trouble";
//     }
//     else
//     {
//         return "single trouble";
//     }
// }
//     }
// }

