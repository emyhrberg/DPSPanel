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
using DPSPanel.Core.Configs;
using Terraria.GameContent;
using ReLogic.Graphics;
using DPSPanel.Core.Panel;
using DPSPanel.Core.Helpers;
using Terraria.Social.WeGame;

namespace DPSPanel.Core.Panel
{
    /// <summary>
    /// Handles the panel, buttons, and logic for the DPS panel.
    /// Shows the DPS panel when the player presses the toggle keybind.
    /// </summary>
    public class PanelState : UIState
    {
        // Panel variables
        public Panel panel;
        public bool isVisible = true;

        //btn
        public KingSlimeButton ksButton;

        public override void OnInitialize()
        {
            base.OnInitialize();

            // LOAD all resources from LoadResources.cs
            LoadResources.PreloadResources();

            // add panel
            panel = new Panel();
            Append(panel);
            panel.AddBossTitle("Fight a boss to show stats!");

            // add button ON TOP
            ksButton = new KingSlimeButton();
            Append(ksButton);

            ToggleDPSPanel(); // turn off by default
        }

        public void ToggleDPSPanel()
        {
            isVisible = !isVisible;
            if (isVisible)
            {
                // Show
                Append(panel);
            }
            else
            {
                // Hide
                panel.Remove();
            }
        }
    }
}