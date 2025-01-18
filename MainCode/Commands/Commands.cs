using System;
using DPSPanel.MainCode.Panel;
using log4net.Repository.Hierarchy;
using Terraria.ModLoader;

namespace DPSPanel.MainCode.Commands
{
    public class Commands : ModCommand
    {
        public override CommandType Type => CommandType.Chat; // Makes the command available in chat
        public override string Command => "dps"; // The main command is "/dps"
        public override string Usage => "Use: /dps"; // Usage instructions
        public override string Description => "Usage: /dps toggle"; // Is shown when using "/help"

        private int i = 0;

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            if (args.Length < 1)
            {
                throw new UsageException("Error: Please enter an argument. Valid arguments are: /dps hide, show, clear.");
            }

            // Access the UISystem to manage the panels
            var uiSystem = ModContent.GetInstance<PanelSystem>();

            // Determine the target
            string target = args[0].ToLower(); // "dps" or "panel"

            if (target == "toggle")
            {
                uiSystem.state.ToggleDPSPanel();
            }
            else if (target == "clear")
            {
                // Broken
                //uiSystem.state.panel.ClearPanelAndAllItems();
                //uiSystem.state.panel.AddBossTitle("Boss Name");
            }
            else
            {
                throw new UsageException("Error: Incorrect argument. Valid arguments are: /dps hide, show, clear.");
            }
        }
    }
}
