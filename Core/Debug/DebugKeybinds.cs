//#if DEBUG
//using DPSPanel.Core.Utilities;
//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Input;
//using Terraria;
//using Terraria.ID;
//using Terraria.ModLoader;

//namespace DPSPanel.Core.Debug;

//[Autoload(Side = ModSide.Client)]
//internal sealed class DebugKeybindPlayer : ModPlayer
//{
//    private const string Banner =
//        "--------- DEBUG KEYBINDS (DPSPANEL) -----------\n" +
//        "Numpad1: " +
//        "Numpad2: " +
//        "Numpad3: ";

//    public override void OnEnterWorld()
//    {
//        if (Main.dedServ || Player.whoAmI != Main.myPlayer)
//            return;

//        Main.NewText(Banner, DebugKeybinds.MessageColor);
//    }
//}

//[Autoload(Side = ModSide.Client)]
//internal sealed class DebugKeybinds : ModSystem
//{
//    internal static readonly Color MessageColor = new(255, 170, 60);

//    public override void UpdateUI(GameTime gameTime)
//    {
//        if (Main.gameMenu || Main.dedServ)
//            return;

//        if (KeyboardHelper.Pressed(Keys.Add))
//        {
//            AddBot();
//            return;
//        }

//        if (KeyboardHelper.Pressed(Keys.Subtract))
//        {
//            RemoveLastBot();
//            return;

//    }

//    private static void AddBot()
//    {
//        if (!BotManager.CanManageBots())
//            return;

//        if (Main.netMode == NetmodeID.MultiplayerClient)
//        {
//            BotNetPacketHandler.SendAddPlayerPacket();
//            return;
//        }

//        if (!BotManager.TryCreate(Main.LocalPlayer, out _, out string message) &&
//            !string.IsNullOrWhiteSpace(message))
//        {
//            Main.NewText(message, Color.OrangeRed);
//        }
//    }

//    private static void RemoveLastBot()
//    {
//        if (Main.netMode == NetmodeID.MultiplayerClient)
//        {
//            if (BotManager.TryGetLastBot(out int slot))
//                BotNetPacketHandler.SendRemovePlayerPacket(slot);
//            else
//                Main.NewText("No bot player to remove.", Color.OrangeRed);

//            return;
//        }

//        BotManager.RemoveLast(out _);
//    }
//}
//#endif
