using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using DPSPanel.Debug.DebugActions;
using DPSPanel.Helpers;
using DPSPanel.UI;
using log4net;
using log4net.Appender;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using static DPSPanel.Common.Configs.Config;

namespace DPSPanel.Debug
{
    public class DebugPanel : UIPanel
    {
        private int TextTopOffset = 0;

        // Add players
        private string[] players = ["Vladimir", "Ivan", "Dmitri", "Sergei, Alexei", "Yuri", "Anatoli", "Boris", "Mikhail", "Nikolai", "Pavel"];
        private int i = 0;

        public DebugPanel()
        {
            Width.Set(200, 0);
            Height.Set(400, 0);
            VAlign = 0.8f;
            HAlign = 0.02f; // bottom left corner

            // Add action texts
            AddButton("Open Config", OpenConfig);
            AddButton("Clear Panel", ClearPlayers);
            AddButton("Add Player", AddPlayer);
            AddButton("God", ToggleGod);
            AddButton("Disable enemy spawn", ToggleEnemySpawns);
            AddButton("Open client log", OpenClientLog);
            AddButton("Clear client log", ClearClientLog);
            AddButton("Spawn King Slime", SpawnKingSlimeSP);
            AddButton("Set Spawn Point", SetSpawnPoint);
        }

        #region Actions

        private void SetSpawnPoint()
        {
            // Set spawn point to the current position of the player
            Main.spawnTileX = (int)(Main.LocalPlayer.position.X / 16f);
            Main.spawnTileY = (int)(Main.LocalPlayer.position.Y / 16f);
            Main.NewText("Spawn point set to current position.");
        }

        private void ClearClientLog()
        {
            // Get all file appenders from log4net's repository
            var appenders = LogManager.GetRepository().GetAppenders().OfType<FileAppender>();

            foreach (var appender in appenders)
            {
                // Close the file to release the lock.
                var closeFileMethod = typeof(FileAppender).GetMethod("CloseFile", BindingFlags.NonPublic | BindingFlags.Instance);
                closeFileMethod?.Invoke(appender, null);

                // Overwrite the file with an empty string.
                // This may not be neccessary, the file may be closed/empty already.
                File.WriteAllText(appender.File, string.Empty);

                // Reactivate the appender so that logging resumes.
                appender.ActivateOptions();
            }
            Main.NewText("Client log cleared.");
        }

        private void SpawnKingSlimeSP()
        {
            // Spawn King Slime in Single Player
            int kingSlimeID = NPCID.KingSlime;
            int x = (int)Main.LocalPlayer.position.X;
            int y = (int)Main.LocalPlayer.position.Y;

            NPC.NewNPCDirect(
                source: null,
                x: x,
                y: y,
                type: kingSlimeID
            );
        }

        public static void OpenClientLog()
        {
            try
            {
                string path = Logging.LogPath;
                string fileName = Path.GetFileName(path);

                Process.Start(new ProcessStartInfo($@"{path}") { UseShellExecute = true });
                Main.NewText("Opening " + fileName + "...");
            }
            catch (Exception ex)
            {
                Main.NewText("Error opening client log: " + ex.Message);
                Log.Error("Error opening client log: " + ex.Message);
            }
        }

        private void ToggleEnemySpawns()
        {
            SpawnRateNPC.DisableSpawns = !SpawnRateNPC.DisableSpawns;
            Main.NewText($"Disable spawn and kill all NPCs is now {(SpawnRateNPC.DisableSpawns ? "enabled" : "disabled")}.");
        }

        private void ToggleGod()
        {
            God.GodEnabled = !God.GodEnabled;
            Main.NewText($"God mode is now {(God.GodEnabled ? "enabled" : "disabled")}.");
        }

        private void AddPlayer()
        {
            // damage = random number between 100 and 2000
            int damage = Main.rand.Next(100, 2000);
            int playerHeadIndex = 0;
            i++;
            if (i >= players.Length)
                i = Main.rand.Next(0, players.Length);
            string randomName = players[i];
            // randomName = "LongName18Characts";
            MainSystem sys = ModContent.GetInstance<MainSystem>();
            sys.state.container.panel.UpdatePlayerBars(randomName, damage, playerHeadIndex, []);
        }

        private void AddButton(string text, Action onClick)
        {
            DebugButton button = new(text, onClick);
            button.Top.Set(TextTopOffset, 0);
            TextTopOffset += 30; // Increment the top position for the next button
            Append(button);
        }

        private void OpenConfig()
        {
            Main.NewText("Opening config menu...");
            Conf.C.Open();
        }

        private void ClearPlayers()
        {
            Main.NewText("Cleared all items in the panel!");
            MainSystem sys = ModContent.GetInstance<MainSystem>();
            sys.state.container.panel.ClearPanelAndAllItems();
            sys.state.container.panel.SetBossTitle("DPSPanel", -1, -1);
        }
        #endregion

        #region Dragging
        private bool dragging;
        private Vector2 offset;

        public override void LeftMouseDown(UIMouseEvent evt)
        {
            base.LeftMouseDown(evt);
            if (ContainsPoint(evt.MousePosition))
            {
                dragging = true;
                offset = evt.MousePosition - new Vector2(Left.Pixels, Top.Pixels);
            }
        }
        public override void LeftMouseUp(UIMouseEvent evt)
        {
            base.LeftMouseUp(evt);
            dragging = false;
        }
        public override void Update(GameTime gameTime)
        {


            if (ContainsPoint(Main.MouseScreen))
            {
                Main.LocalPlayer.mouseInterface = true; // disable item use on clicking this panel
            }

            base.Update(gameTime);
            if (dragging)
            {
                Left.Set(Main.mouseX - offset.X, 0);
                Top.Set(Main.mouseY - offset.Y, 0);
                Recalculate();
            }
            return;

        }
        #endregion

        #region Right click hide
        private bool Active = true;
        public override void RightClick(UIMouseEvent evt)
        {
            // base.RightClick(evt);
            Active = !Active;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Active)
            {
                return;
            }

            base.Draw(spriteBatch);
        }

        #endregion
    }
}
