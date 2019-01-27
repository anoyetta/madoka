using System.Windows;
using MahApps.Metro.Controls;

namespace madoka.Common
{
    public static class WPFHelper
    {
        public static MetroWindow MainWindow => Application.Current?.MainWindow as MetroWindow;

#if DEBUG
        private static bool isDebugMode = false;
#endif

        public static bool IsDesignMode
        {
            get
            {
#if DEBUG
                if (!isDebugMode)
                {
                    if (System.ComponentModel.LicenseManager.UsageMode ==
                        System.ComponentModel.LicenseUsageMode.Designtime)
                    {
                        isDebugMode = true;
                    }
                    else
                    {
                        using (var p = System.Diagnostics.Process.GetCurrentProcess())
                        {
                            isDebugMode =
                                p.ProcessName.Equals("DEVENV", System.StringComparison.OrdinalIgnoreCase) ||
                                p.ProcessName.Equals("XDesProc", System.StringComparison.OrdinalIgnoreCase);
                        }
                    }
                }

                return isDebugMode;
#else
                return false;
#endif
            }
        }
    }
}
