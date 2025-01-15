using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;
using Terraria.UI;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Microsoft.Xna.Framework;

namespace BetterDPS.UI.DPS
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
        private bool isVisible = true;

        public override void OnInitialize()
        {
            // Add the draggable panel which shows dps
            dpsPanel = new DPSPanel();

            // Add play button to the panel
            Asset<Texture2D> buttonPlayTexture = ModContent.Request<Texture2D>("Terraria/Images/UI/ButtonPlay");
            DPSPanelHoverButton buttonPlay = new DPSPanelHoverButton(buttonPlayTexture, "Calculate DPS");
            SetRectangle(buttonPlay, 0, 0, 40, 40); // top left corner
            buttonPlay.OnLeftClick += new MouseEvent(PlayButtonClicked);
            dpsPanel.Append(buttonPlay);

            // Add close button to the panel
            // this one loads from Assets/ButtonClose.png
            Asset<Texture2D> buttonCloseTexture = ModContent.Request<Texture2D>("BetterDPS/Content/Assets/ButtonClose");
            DPSPanelHoverButton buttonClose = new DPSPanelHoverButton(buttonCloseTexture, "Close");
            SetRectangle(buttonClose, 300-40, 0, 40, 40); // top right corner
            buttonClose.OnLeftClick += new MouseEvent(CloseButtonClicked);
            dpsPanel.Append(buttonClose);

            // Add the items
            for (int i = 1; i <= 6; i++)
            {
                //dpsPanel.AddItem($"Item {i}");
            }

            // Add the entire DPS panel to the UIState
            Append(dpsPanel);
        }

        private void PlayButtonClicked(UIMouseEvent evt, UIElement listeningElement)
        {
            SoundEngine.PlaySound(SoundID.MenuOpen);
            // Calculate DPS by getting the player source
            int dps = Main.LocalPlayer.getDPS();
            Main.NewText($"DPS: {dps}");

            // add panel item
            dpsPanel.AddItem($"DPS: {dps}");
        }

        private void CloseButtonClicked(UIMouseEvent evt, UIElement listeningElement)
        {
            SoundEngine.PlaySound(SoundID.MenuClose);
            HideDPSPanel();
        }

        // Helper method to set the position and size of a UIElement
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
            {
                Append(dpsPanel); // Append the panel to the UIState
                Main.NewText("[BetterDPS] DPS Panel enabled.", Color.Green);
            }
        }

        public void HideDPSPanel()
        {
            dpsPanel.Remove();
            Main.NewText("[BetterDPS] DPS Panel disabled. Use /enable dps or 'K' to enable it again.", Color.Red);
        }

        public void ToggleDPSPanel()
        {
            if (isVisible)
            {
                HideDPSPanel();
            }
            else
            {
                ShowDPSPanel();
            }
            isVisible = !isVisible;
        }
    }
}
