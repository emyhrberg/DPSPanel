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
using Terraria.GameContent;
using ReLogic.Graphics;

namespace DPSPanel.MainCode.Panel
{
    /// <summary>
    /// Handles the panel, buttons, and logic for the DPS panel.
    /// Shows the DPS panel when the player presses the toggle keybind.
    /// </summary>
    public class PanelState : UIState
    {
        // Panel variables
        public Panel panel;
        private bool isVisible = true;

        // Panel settings
        private readonly float padding = 5f;
        private readonly float w = 200f; // PANEL WIDTH
        private readonly float h = 300f; // PANEL HEIGHT. is reset later anyways
        private readonly float leftOffset = 15f; // place under inventory for now
        private readonly float topOffset = 260f; // place under inventory for now
        private readonly Color panelColor = new(56, 58, 134); // Light blue, same as inventory panel

        // Button settings
        private readonly float buttonSize22px = 22f;

        public override void OnInitialize()
        {
            base.OnInitialize();
            panel = CreatePanel();
            panel.AddBossTitle("Boss Name");
            //InitializeButtons(panel);
        }

        /* -------------------------------------------------------------
         * Panel setup code
         * -------------------------------------------------------------
         */

        private Panel CreatePanel()
        {
            panel = new Panel(padding);

            // Size of panel
            panel.Width.Set(w, 0f);
            panel.Height.Set(h, 0f);

            // Position of panel
            panel.Left.Set(leftOffset, 0f); // distance from the left edge
            panel.Top.Set(topOffset, 0f); // distance from the top edge

            // Background color of panel
            panel.BackgroundColor = panelColor; // Light blue background
            Append(panel);
            return panel;
        }

        /* -------------------------------------------------------------
         * Button setup code
         * -------------------------------------------------------------
         */

        public void InitializeButtons(Panel dpsPanel)
        {
            // --- Add play button to the panel
            //Asset<Texture2D> buttonDeleteTexture = ModContent.Request<Texture2D>("Terraria/Images/UI/ButtonDelete");
            //PanelHoverButton buttonDelete = new(buttonDeleteTexture, "Clear");
            //buttonDelete.OnLeftClick += new MouseEvent(DeleteButtonClicked);
            //dpsPanel.Append(buttonDelete);

            // --- Add close button to the panel
            //this one loads from Assets/ ButtonClose.png
            Asset<Texture2D> buttonCloseTexture = ModContent.Request<Texture2D>("DPSPanel/MainCode/Assets/ButtonClose");
            PanelHoverButton buttonClose = new(buttonCloseTexture, "Close");
            buttonClose.Height.Set(buttonSize22px, 0);
            buttonClose.Width.Set(buttonSize22px, 0);
            //buttonClose.Left.Set(-buttonSize / 2, 0);
            //buttonClose.Top.Set(-buttonSize / 2, 0);
            buttonClose.OnLeftClick += new MouseEvent(CloseButtonClicked);
            dpsPanel.Append(buttonClose);
        }

        private void DeleteButtonClicked(UIMouseEvent evt, UIElement listeningElement)
        {
        }

        private void CloseButtonClicked(UIMouseEvent evt, UIElement listeningElement)
        {
            SoundEngine.PlaySound(SoundID.MenuClose);
            HideDPSPanel();
        }

        /* -------------------------------------------------------------
         * Toggle panel methods
         * -------------------------------------------------------------
         */

        public void ShowDPSPanel()
        {
            if (!Children.Contains(panel))
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
                Append(panel); // Append the panel to the UIState
                Main.NewText("Damage Panel: [ON]. Press K to toggle.", Color.Green);
            }
        }

        public void HideDPSPanel()
        {
            isVisible = false;
            panel.Remove();
            Main.NewText("Damage Panel: [OFF]. Press K to toggle.", Color.Red);
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
