using Terraria.ModLoader;

namespace DPSPanel.Helpers
{
    public static class Log
    {
        private static Mod ModInstance => ModContent.GetInstance<DPSPanel>();

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

        public static void SlowInfo(string message, float delay = 0.5f)
        {
            if (TimeHelper.ToSeconds(delay).TotalSeconds > 0.5f)
            {
                ModInstance.Logger.Info($"[SlowInfo] {message}");
                return;
            }
        }
    }
}
