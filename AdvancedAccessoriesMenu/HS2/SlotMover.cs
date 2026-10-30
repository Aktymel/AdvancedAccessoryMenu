using CharaCustom;
using HarmonyLib;
using KKAPI.Maker;
using UnityEngine.UI;

namespace AdvancedAccessoriesMenu
{
    // Moves an accessory with the game's own Copy page and then empties the source slot like choosing "None".
    // KKAPI reports both to other plugins, so Material Editor and the like move their data along.
    internal static class SlotMover
    {
        public static bool Available => true;

        public static string Warning => null;

        public static bool Move(int source, int destination)
        {
            var slot = ScreenUi.FindInScene<CvsA_Slot>();
            if (slot == null || !Copy(source, destination)) return false;

            // The slot page works on the slot in SNo; it goes back to the selected slot afterwards
            slot.SNo = source;
            slot.ChangeAcsType(0);
            slot.SNo = AccessoriesApi.SelectedMakerAccSlot;
            slot.UpdateCustomUI();
            return true;
        }

        public static bool Copy(int source, int destination)
        {
            var copy = ScreenUi.FindInScene<CvsA_Copy>();
            if (copy == null) return false;

            var page = Traverse.Create(copy);
            var oldSource = page.Field("selSrc").GetValue<int>();
            var oldDestination = page.Field("selDst").GetValue<int>();
            var mirror = page.Field("tglChgParentLR").GetValue<Toggle>();
            var mirrored = mirror != null && mirror.isOn;
            try
            {
                if (mirrored) mirror.isOn = false;
                page.Field("selSrc").SetValue(source);
                page.Field("selDst").SetValue(destination);
                AccessTools.Method(typeof(CvsA_Copy), "CopyAccessory").Invoke(copy, null);
            }
            finally
            {
                page.Field("selSrc").SetValue(oldSource);
                page.Field("selDst").SetValue(oldDestination);
                if (mirrored) mirror.isOn = true;
            }
            return true;
        }
    }
}
