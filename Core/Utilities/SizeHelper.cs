using DPSPanel.Common.Configs;
using DPSPanel.UI;

namespace DPSPanel.Core.Utilities;

public static class SizeHelper
{
    public static float GetWidthFromConfig() => DPSPanelLayout.WidthFor(Config.Conf.C?.Width);
}
