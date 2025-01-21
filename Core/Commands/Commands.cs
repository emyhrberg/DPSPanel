using System;
using DPSPanel.Core.Panel;
using DPSPanel.MainCode.Panel;
using log4net.Repository.Hierarchy;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace DPSPanel.Core.Commands
{
    public class Commands : ModCommand
    {
        public override CommandType Type => CommandType.Chat; // Makes the command available in chat
        public override string Command => "dps"; // The main command is "/dps"
        public override string Usage => "Use: /dps"; // Usage instructions
        public override string Description => "Usage: /dps toggle"; // Is shown when using "/help"

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            if (args.Length < 1)
            {
                throw new UsageException("Error: Please enter an argument. Valid arguments are: /dps hide, show, clear.");
            }

            var sys = ModContent.GetInstance<PanelSystem>();
            string target = args[0].ToLower(); // "dps" or "panel"
            if (target == "toggle")
            {
                //sys.state.group.ToggleDPSPanel();
                //KingSlimeButton.ShowButton = !KingSlimeButton.ShowButton;
            }
            else
            {
                throw new UsageException("Error: Incorrect argument. Valid arguments are: /dps hide, show, clear.");
            }
        }
    }

    public class ReverseCommand : ModCommand
    {
        public override CommandType Type => CommandType.Chat; // Makes the command available in chat
        public override string Command => "toggle"; // The main command is "/dps"
        public override string Usage => "Use: /dps"; // Usage instructions
        public override string Description => "Usage: /toggle dps"; // Is shown when using "/help"

        private int i = 0;

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            if (args.Length < 1)
            {
                throw new UsageException("Error: Please enter an argument. Valid arguments are: /dps hide, show, clear.");
            }
            var panelSys = ModContent.GetInstance<PanelSystem>();
            string target = args[0].ToLower(); // "dps" or "panel"

            if (target == "dps")
            {
                //bool vis = panelSys.state.panelVisible;
                //string onOff = vis ? "ON" : "OFF";
                //Main.NewText($"Damage Panel: [{onOff}]. Press K to toggle.", Color.Yellow);
                //panelSys.state.ToggleDPSPanel();
                //KingSlimeButton.ShowButton = !KingSlimeButton.ShowButton;
            }
            else
            {
                throw new UsageException("Error: Incorrect argument. Valid arguments are: /dps hide, show, clear.");
            }
        }
    }
}
