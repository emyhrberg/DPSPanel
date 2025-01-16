using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using DPSPanel.MainCode.Panel;

namespace DPSPanel.MainCode.Commands
{
    public class Commands : ModCommand
    {
        public override CommandType Type => CommandType.Chat; // Makes the command available in chat
        public override string Command => "dps"; // The main command is "/dps"
        public override string Usage => "Use: /dps"; // Usage instructions
        public override string Description => "Usage: /dps show /dps hide /dps clear"; // Is shown when using "/help"

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            if (args.Length < 1)
            {
                throw new UsageException("Error: Please enter an argument");
            }

            // Access the UISystem to manage the panels
            var uiSystem = ModContent.GetInstance<DPSPanelSystem>();

            // Determine the target
            string target = args[0].ToLower(); // "dps" or "panel"

            if (target == "hide")
            {
                uiSystem.state.HideDPSPanel();
            }
            else if (target == "show")
            {
                uiSystem.state.ShowDPSPanel();
            }
            else if (target == "clear")
            {
                uiSystem.state.ClearDPSPanel();
            }

            else
            {
                throw new UsageException("Error: Incorrect argument inputted");
            }
        }
    }
}
