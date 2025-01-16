using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using DPSPanel.UI.DPS;

namespace DPSPanel.UI.Commands
{
    public class ClearCommand : ModCommand
    {
        public override CommandType Type => CommandType.Chat; // Makes the command available in chat
        public override string Command => "clear"; // The main command is "/enable"
        public override string Usage => "Use: /clear dps"; // Usage instructions
        public override string Description => "Clear DPS UI panels with /clear dps";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            if (args.Length < 1)
            {
                usageException();
            }

            // Access the UISystem to manage the panels
            var uiSystem = ModContent.GetInstance<DPSPanelSystem>();

            // Determine the target
            string target = args[0].ToLower(); // the target is the first argument

            if (target == "dps")
            {
                uiSystem.state.dpsPanel.clearAllItems();
            }
            else
            {
                usageException();
            }
        }

        private void usageException()
        {
            throw new UsageException("Faulty command. Use: /clear dps");
        }
    }
}
