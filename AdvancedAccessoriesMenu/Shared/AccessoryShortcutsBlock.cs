using HarmonyLib;
using UnityEngine;

namespace AdvancedAccessoriesMenu
{
    // Accessory Shortcuts (KKS) puts an accessory into the selected empty slot when a digit is typed. It reads the
    // digit from Input.inputString, which InputHotkeyBlock doesn't block, so typing a slot number in the menu
    // changed accessories. Its shortcuts are skipped while an IMGUI text field has the keyboard.
    internal static class AccessoryShortcutsBlock
    {
        public static void Patch(Harmony harmony)
        {
            var type = AccessTools.TypeByName("Accessory_Shortcuts.CharaEvent");
            var update = type == null ? null : AccessTools.Method(type, "Update");
            if (update != null) harmony.Patch(update, prefix: new HarmonyMethod(typeof(AccessoryShortcutsBlock), nameof(UpdatePrefix)));
        }

        private static bool UpdatePrefix()
        {
            return GUIUtility.keyboardControl == 0;
        }
    }
}
