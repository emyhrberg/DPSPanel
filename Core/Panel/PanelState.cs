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
    public class PanelState : UIState
    {
        public Panel panel;
        public bool isVisible = true;

        public override void OnInitialize()
        {
            base.OnInitialize();

            // LOAD all resources from LoadResources.cs
            LoadResources.PreloadResources();

            // add panel
            panel = new Panel();
            Append(panel);
            panel.AddBossTitle("Fight a boss to show stats!");

            //ToggleDPSPanel(); // disable by default
        }

        public void ToggleDPSPanel()
        {
            isVisible = !isVisible;
            if (isVisible)
            {
                Append(panel);
            }
            else
                panel.Remove();
        }
    }
}