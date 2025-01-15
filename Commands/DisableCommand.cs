using Terraria;
using Terraria.ModLoader;
using BetterDPS.UI;
using Microsoft.Xna.Framework; // For Color

namespace BetterDPS.Commands
{
    public class DisableCommand : ModCommand
    {
        public override CommandType Type => CommandType.Chat; // Makes the command available in chat
        public override string Command => "disable"; // The main command is "/disable"
        public override string Usage => "Use: /disable dps"; // Usage instructions
        public override string Description => "Disable DPS UI panel.";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            if (args.Length < 1)
            {
                throw new UsageException("[BetterDPS] Use: /disable dps");
            }

            // Access the UISystem to manage the panels
            var uiSystem = ModContent.GetInstance<DPSPanelSystem>();

            // Determine the target
            string target = args[0].ToLower(); // "dps" or "panel"

            if (target == "dps")
            {
                uiSystem.container.HideDPSPanel();
                Main.NewText("[BetterDPS] DPS Panel disabled.", Color.Red);
            }
            else
            {
                throw new UsageException("[BetterDPS] Invalid target. Use: /disable dps");
            }
        }
    }
}
