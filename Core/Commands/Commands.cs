using DPSPanel.Core.Panel;
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
                throw new UsageException("Error: Please enter an argument. Valid arguments are: /dps hide, show, clear.");

            var sys = ModContent.GetInstance<PanelSystem>();

            string target = args[0].ToLower(); // "dps" or "panel"
            if (target == "toggle")
                sys?.state.container.TogglePanel();
            else
                throw new UsageException("Error: Incorrect argument. Valid arguments are: /dps hide, show, clear.");
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
                throw new UsageException("Error: Please enter an argument. Valid arguments are: /dps hide, show, clear.");
            var sys = ModContent.GetInstance<PanelSystem>();
            string target = args[0].ToLower(); // "dps" or "panel"

            if (target == "dps")
                sys?.state.container.TogglePanel();
            else
                throw new UsageException("Error: Incorrect argument. Valid arguments are: /dps hide, show, clear.");
        }
    }
}
