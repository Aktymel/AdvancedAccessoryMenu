using System;
using ChaCustom;
using HarmonyLib;
using UnityEngine.UI;

namespace AdvancedAccessoriesMenu
{
    // Moves an accessory with the game's own Transfer page and then empties the source slot like choosing "None".
    // KKAPI reports both to other plugins, so Material Editor and the like move their data along.
    internal static class SlotMover
    {
        // Off until copying works in KKS / KK: other plugins' patches on CopyAcs break it
        public static bool Available => false;

        public static bool Move(int source, int destination)
        {
            var changeSlot = ScreenUi.FindInScene<CustomAcsChangeSlot>();
            var sourcePage = changeSlot == null ? null : SlotPage(changeSlot, source);
            if (sourcePage == null || !Copy(source, destination)) return false;

            sourcePage.UpdateSelectAccessoryType(0);
            sourcePage.UpdateCustomUI();
            return true;
        }

        // The destination must be empty
        public static bool Copy(int source, int destination)
        {
            var transfer = ScreenUi.FindInScene<CvsAccessoryChange>();
            if (transfer == null) return false;

            var page = Traverse.Create(transfer);
            var oldSource = page.Field("selSrc").GetValue<int>();
            var oldDestination = page.Field("selDst").GetValue<int>();
            var mirror = page.Field("tglReverse").GetValue<Toggle>();
            var mirrored = mirror != null && mirror.isOn;
            try
            {
                if (mirrored) mirror.isOn = false;
                page.Field("selSrc").SetValue(source);
                page.Field("selDst").SetValue(destination);
                // A normal call through a delegate, not MethodBase.Invoke: other plugins' patches on CopyAcs
                // (e.g. Pregnancy Plus) fail with FieldAccessException when it is called by reflection
                var copyAcs = (Action)Delegate.CreateDelegate(typeof(Action), transfer, AccessTools.Method(typeof(CvsAccessoryChange), "CopyAcs"));
                copyAcs();
            }
            finally
            {
                page.Field("selSrc").SetValue(oldSource);
                page.Field("selDst").SetValue(oldDestination);
                if (mirrored) mirror.isOn = true;
            }
            return !MakerSlots.IsEmpty(destination);
        }

        private static CvsAccessory SlotPage(CustomAcsChangeSlot changeSlot, int slot)
        {
            foreach (var page in changeSlot.GetComponentsInChildren<CvsAccessory>(true))
                if ((int)page.slotNo == slot) return page;
            return null;
        }
    }
}
