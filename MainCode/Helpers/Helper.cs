using Terraria.ModLoader;

namespace DPSPanel.MainCode.Helpers
{
    public static class Helper
    {
        public static void throwUsageException(string message)
        {
            throw new UsageException(message);
        }
    }
}
