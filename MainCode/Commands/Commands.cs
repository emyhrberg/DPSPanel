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
        public override string Description => "Usage: /dps show /dps hide /dps clear"; // Is shown when using "/help"

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

            if (target == "hide")
            {
                uiSystem.state.HideDPSPanel();
            }
            else if (target == "show")
            {
                uiSystem.state.ShowDPSPanel();
            }
            else if (target == "a")
            {
                //uiSystem.state.panel.AddPanelItem($"Item {i}");
                string text = $"Item {i}";
                uiSystem.state.panel.AddSlider(text, 95-i*10);
                i++;
            }
            else
            {
                throw new UsageException("Error: Incorrect argument. Valid arguments are: /dps hide, show, clear.");
            }
        }
    }
}
