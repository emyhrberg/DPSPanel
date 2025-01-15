using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;
using Terraria.ModLoader.UI;
using Terraria.UI;
using Terraria.GameContent.UI.Elements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace BetterDPS.UI
{
    /// <summary>
    /// Represents a container for UI elements in Terraria's modding framework.
    /// </summary>
    /// <remarks>
    /// The UIState serves as a canvas where multiple UI elements (e.g., panels, buttons, labels) can be added and managed.
    /// It handles the organization and updating of all child elements and is used in conjunction with the UISystem to render
    /// custom interfaces in the game.
    /// </remarks>
    public class DPSPanelState : UIState
    {
        // Variables
        private DPSPanel dpsPanel;

        public override void OnInitialize()
        {
            // Add the draggable panel which shows dps
            dpsPanel = new DPSPanel();

            // Add play button to the panel
            Asset<Texture2D> buttonPlayTexture = ModContent.Request<Texture2D>("Terraria/Images/UI/ButtonPlay");
            DPSPanelHoverButton buttonPlay = new DPSPanelHoverButton(buttonPlayTexture, "Calculate DPS");
            SetRectangle(buttonPlay, 0, 0, 40, 40); // top, left, width, height
            dpsPanel.Append(buttonPlay);

            // Add the entire DPS panel to the UIState
            Append(dpsPanel);
        }

        private void SetRectangle(UIElement uiElement, float left, float top, float width, float height)
        {
            uiElement.Left.Set(left, 0f);
            uiElement.Top.Set(top, 0f);
            uiElement.Width.Set(width, 0f);
            uiElement.Height.Set(height, 0f);
        }

        // Methods to toggle DPS Panel
        public void ShowDPSPanel()
        {
            if (!Children.Contains(dpsPanel))
                Append(dpsPanel);
        }

        public void HideDPSPanel()
        {
            dpsPanel.Remove();
        }
    }
}
