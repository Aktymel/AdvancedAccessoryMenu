using System.Diagnostics;
using System.Text;
using KKAPI.Maker;

namespace AdvancedAccessoriesMenu
{
    // Diagnostic log for testing. Calls to it are left out of Release builds by the compiler, so it can stay in the code.
    internal static class Diag
    {
        [Conditional("DEBUG")]
        public static void Log(string message)
        {
            AdvancedAccessoriesMenuPlugin.Logger.LogWarning("[diag] " + message);
        }

        // Every slot that isn't empty, as slot=type/id
        [Conditional("DEBUG")]
        public static void LogSlots(string step)
        {
            var sb = new StringBuilder(step).Append(':');
            var count = AccessoriesApi.GetMakerAccessoryCount();
#pragma warning disable 612
            for (var slot = 0; slot < count; slot++)
            {
                var part = AccessoriesApi.GetPartsInfo(slot);
                if (!MakerSlots.IsEmpty(slot)) sb.Append(' ').Append(slot + 1).Append('=').Append(part.type).Append('/').Append(part.id);
            }
#pragma warning restore 612
            Log(sb.ToString());
        }
    }
}
