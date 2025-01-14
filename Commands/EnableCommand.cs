using Terraria;
using Terraria.ModLoader;
using BetterDPS.UI;
using Microsoft.Xna.Framework; // For Color

namespace BetterDPS.Commands
{
    public class EnableCommand : ModCommand
    {
        public override CommandType Type => CommandType.Chat; // Makes the command available in chat
        public override string Command => "enable"; // The main command is "/enable"
        public override string Usage => "Usage: /enable dps or /enable panel"; // Usage instructions
        public override string Description => "Enable specific UI panels.";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            if (args.Length < 1)
            {
                throw new UsageException("Usage: /enable dps or /enable panel");
            }

            // Access the UISystem to manage the panels
            var uiSystem = ModContent.GetInstance<UISystem>();

            // Determine the target
            string target = args[0].ToLower(); // "dps" or "panel"

            if (target == "dps")
            {
                uiSystem.container.ShowDPSPanel();
                Main.NewText("DPS Panel enabled.", Color.Green);
            }
            else if (target == "panel")
            {
                uiSystem.container.ShowExamplePanel();
                Main.NewText("Example Panel enabled.", Color.Green);
            }
            else
            {
                throw new UsageException("Invalid target. Usage: / enable dps or / enable panel");
            }
        }
    }
}
