using System;
using System.IO;
using System.Runtime.CompilerServices;
using Terraria.ModLoader;

namespace DPSPanel.Helpers
{
    public static class Log
    {
        private static Mod ModInstance => ModContent.GetInstance<DPSPanel>();
        private static DateTime lastLogTime = DateTime.MinValue;

        public static void Info(string message)
        {
            ModInstance.Logger.Info(message);
        }

        public static void Warn(string message)
        {
            ModInstance.Logger.Warn(message);
        }

        public static void Error(string message)
        {
            ModInstance.Logger.Error(message);
        }

        public static void SlowInfo(string message, int seconds = 1, bool debug = false, [CallerFilePath] string callerFilePath = "")
        {
            // Extract the class name from the caller's file path.
            string className = Path.GetFileNameWithoutExtension(callerFilePath);
            var instance = ModInstance;
            if (instance == null || instance.Logger == null)
                return; // Skip logging if the mod is unloading or null

            // Use TimeSpanFactory to create a 3-second interval.
            TimeSpan interval = TimeHelper.FromSeconds(seconds);
            if (DateTime.UtcNow - lastLogTime >= interval)
            {
                // Prepend the class name to the log message.
                string formattedMessage = debug ? $"[{className}] {message}" : message;
                instance.Logger.Info(formattedMessage);
                lastLogTime = DateTime.UtcNow;
            }
        }
    }
}
