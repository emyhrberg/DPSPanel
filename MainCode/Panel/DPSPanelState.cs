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
using System.Collections.Generic;
using DPSPanel.MainCode.Configs;

namespace DPSPanel.MainCode.Panel
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
        public DPSPanel dpsPanel;
        private bool isVisible = true;

        public override void OnInitialize()
        {
            dpsPanel = new DPSPanel();
            initializeButtons(dpsPanel);
            Append(dpsPanel);
        }

        /*
         * Button methods
         */////////////////////////////////////////////////////////////////////////////////////

        private void initializeButtons(DPSPanel dpsPanel)
        {
            // Add the draggable panel which shows dps

            // Add play button to the panel
            //Asset<Texture2D> buttonPlayTexture = ModContent.Request<Texture2D>("Terraria/Images/UI/ButtonPlay");
            //DPSPanelHoverButton buttonPlay = new DPSPanelHoverButton(buttonPlayTexture, "Calculate DPS");
            //SetRectangle(buttonPlay, 0, 0, 40, 40); // top left corner
            //buttonPlay.OnLeftClick += new MouseEvent(PlayButtonClicked);
            //dpsPanel.Append(buttonPlay);

            // Add close button to the panel
            // this one loads from Assets/ButtonClose.png
            Asset<Texture2D> buttonCloseTexture = ModContent.Request<Texture2D>("DPSPanel/MainCode/Assets/ButtonClose");
            DPSPanelHoverButton buttonClose = new DPSPanelHoverButton(buttonCloseTexture, "Close");
            SetRectangle(buttonClose, 320 - 40, 0, 40, 40); // top right corner
            buttonClose.OnLeftClick += new MouseEvent(CloseButtonClicked);
            dpsPanel.Append(buttonClose);

        }

        private void PlayButtonClicked(UIMouseEvent evt, UIElement listeningElement)
        {
            SoundEngine.PlaySound(SoundID.MenuOpen);
            // Calculate DPS by getting the player source
            int dps = Main.LocalPlayer.getDPS();
            Main.NewText($"DPS: {dps}");

            // add panel item
            //dpsPanel.AddItem($"DPS: {dps}");
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

        /*
         * Methods to toggle DPS Panel
         */////////////////////////////////////////////////////////////////////////////////////
        public void ClearDPSPanel()
        {
            dpsPanel.ClearItems();
            Main.NewText("CLEAR dps panel.", Color.Green);
        }

        public void ShowDPSPanel()
        {
            if (!Children.Contains(dpsPanel))
            {
                // get keybind
                //KeybindSystem.toggleDPSPanelKeybind.GetAssignedKeys();
                 //get the keybind for toggleDPSPanel
                //if (KeybindSystem.toggleDPSPanelKeybind.GetAssignedKeys().Count == 0)
                //{
                    //Main.NewText("No keybind assigned for toggleDPSPanel. Please assign a keybind in the controls menu.", Color.Red);
                    //return;
                //}

                isVisible = true;
                Append(dpsPanel); // Append the panel to the UIState
                Main.NewText("SHOW dps panel. Press K to toggle.", Color.Green);
            }
        }

        public void HideDPSPanel()
        {
            isVisible = false;
            dpsPanel.Remove();
            Main.NewText("HIDE dps panel. Press K to toggle.", Color.Red);
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
        }
    }
}
