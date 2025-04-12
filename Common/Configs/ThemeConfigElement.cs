using DPSPanel.Common.Systems;
using DPSPanel.Helpers;
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
    /// <see cref="Terraria.ModLoader.Config.UI.StringOptionElement"/> 
    /// And Starlight River CustomConfigElement
    /// https://github.com/ProjectStarlight/StarlightRiver/blob/master/Content/GUI/Config/AbilityUIReposition.cs#L10
    /// </summary>
    public class ThemeConfigElement : StringOptionElement
    {
        // Called once when the config UI binds this element to your Width property
        public override void OnBind()
        {
            base.OnBind();
        }

        public override void OnInitialize()
        {
            base.OnInitialize();
            Height.Set(80, 0);
            Recalculate();
        }

        public override void Draw(SpriteBatch sb)
        {
            base.Draw(sb);

            string selectedTheme = getValue();

            // Draw the currently selected bar theme
            Texture2D previewTexture = selectedTheme switch
            {
                "Default" => Ass.Default.Value,
                "Fancy" => Ass.Fancy.Value,
                "Golden" => Ass.Golden.Value,
                "Leaf" => Ass.Leaf.Value,
                "Retro" => Ass.Retro.Value,
                "Sticks" => Ass.Sticks.Value,
                "StoneGold" => Ass.StoneGold.Value,
                "Tribute" => Ass.Tribute.Value,
                "TwigLeaf" => Ass.TwigLeaf.Value,
                "Valkyrie" => Ass.Valkyrie.Value,
                _ => null
            };

            if (previewTexture != null)
            {
                CalculatedStyle dims = GetDimensions();
                // Draw the selected Large theme near the bottom
                Rectangle destRect = new Rectangle((int)dims.X + 170, (int)dims.Y - 2, 138, 34);
                sb.Draw(previewTexture, destRect, Color.White);
            }
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

