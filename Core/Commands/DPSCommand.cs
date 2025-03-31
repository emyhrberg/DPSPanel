using DPSPanel.UI;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace DPSPanel.Core.Configs
{
    public class DPSCommand : ModCommand
    {
        public override CommandType Type => CommandType.Chat; // Makes the command available in chat
        public override string Command => "dps"; // The main command is "/dps"
        public override string Description => "Usage: /dps <add> <clear> <toggle>"; // Is shown when using "/help"
        public override string Usage => "/dps <add> <clear> <toggle>"; // idk what this does

        // create list of example players using real funny russian names. 10 players
        private string[] players = ["Vladimir", "Ivan", "Dmitri", "Sergei, Alexei", "Yuri", "Anatoli", "Boris", "Mikhail", "Nikolai", "Pavel"];
        int i = 0;

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            var sys = ModContent.GetInstance<MainSystem>();

            // validate args
            if (args.Length != 1)
            {
                throw new UsageException("Error: Please provide an argument. Usage: /dps <add> <clear> <toggle>");
            }

            string target = args[0];

            if (target == "add")
            {
                // damage = random number between 100 and 2000
                int damage = Main.rand.Next(100, 2000);
                int playerHeadIndex = 0;
                i++;
                if (i >= players.Length)
                    i = Main.rand.Next(0, players.Length);
                string randomName = players[i];
                // randomName = "LongName18Characts";
                sys.state.container.panel.UpdatePlayerBars(randomName, damage, playerHeadIndex, []);
            }
            else if (target == "clear")
            {
                sys.state.container.panel.ClearPanelAndAllItems();
                sys.state.container.panel.SetBossTitle("DPSPanel", -1, -1);
            }
            else if (target == "toggle")
            {
                ModContent.GetInstance<Config>().ShowOnlyWhenInventoryOpen = !ModContent.GetInstance<Config>().ShowOnlyWhenInventoryOpen;

                string text = ModContent.GetInstance<Config>().ShowOnlyWhenInventoryOpen ? "Always show DPSPanel" : "Show DPSPanel only when inventory is open";
                Main.NewText(text, Color.White);
            }
            else
            {
                throw new UsageException($"Error: Argument '{target}' not found. Usage: /dps <add> <clear> <toggle>");
            }
        }
    }
}
