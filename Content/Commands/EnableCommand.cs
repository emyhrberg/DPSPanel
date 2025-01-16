using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using DPSPanel.UI.DPS;

namespace DPSPanel.UI.Commands
{
    public class EnableCommand : ModCommand
    {
        public override CommandType Type => CommandType.Chat; // Makes the command available in chat
        public override string Command => "enable"; // The main command is "/enable"
        public override string Usage => "Use: /enable dps"; // Usage instructions
        public override string Description => "Enable DPS UI panels with /enable dps";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            if (args.Length < 1)
            {
                throw new UsageException("[DPSPanel] Use: /enable dps");
            }

            // Access the UISystem to manage the panels
            var uiSystem = ModContent.GetInstance<DPSPanelSystem>();

            // Determine the target
            string target = args[0].ToLower(); // the target is the first argument

            if (target == "dps")
            {
                uiSystem.state.ShowDPSPanel();
            }
            else
            {
                throw new UsageException("[DPSPanel] Invalid target. Use: /enable dps");
            }
        }
    }
}
