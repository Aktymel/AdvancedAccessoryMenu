using System;
using ChaCustom;
using HarmonyLib;
using Object = UnityEngine.Object;

namespace AdvancedAccessoriesMenu
{
    internal static class Hooks
    {
        public static void PatchMoreAccessories(Harmony harmony)
        {
            var type = AccessTools.TypeByName("MoreAccessoriesKOI.Accessories");
            var method = type == null ? null : AccessTools.Method(type, "AddSlot");
            if (method != null) harmony.Patch(method, postfix: new HarmonyMethod(typeof(Hooks), nameof(AddSlotPostfix)));
        }

        // MoreAccessories shows slot buttons kept from the previous character again without renaming them,
        // so they still show that character's accessories
        private static void AddSlotPostfix()
        {
            // Must never break MoreAccessories' own "+1" / "+10"
            try
            {
                var changeSlot = Object.FindObjectOfType<CustomAcsChangeSlot>();
                if (changeSlot == null) return;
                foreach (var cvs in changeSlot.GetComponentsInChildren<CvsAccessory>(true))
                    cvs.UpdateSlotName();
            }
            catch (Exception e)
            {
                AdvancedAccessoriesMenuPlugin.Logger.LogWarning("Could not update slot names: " + e);
            }
        }
    }
}
