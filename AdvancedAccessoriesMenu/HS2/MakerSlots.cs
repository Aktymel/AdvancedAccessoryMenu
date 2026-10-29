using System.Collections;
using CharaCustom;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace AdvancedAccessoriesMenu
{
    internal static class MakerSlots
    {
        private const int VanillaSlotCount = 20;
        private const int AccessoryMenuIndex = 4;

        private static CustomChangeMainMenu _mainMenu;
        private static Button[] _vanillaButtons;
        private static bool _moreAccessoriesSearched;
        private static object _moreAccessories;

        public static void Reset()
        {
            _mainMenu = null;
            _vanillaButtons = null;
            _moreAccessoriesSearched = false;
            _moreAccessories = null;
        }

        public static string Name(int slot)
        {
            var button = GetButton(slot);
            var text = button == null ? null : button.GetComponentInChildren<Text>();
            return text == null ? null : text.text;
        }

        public static void Select(int slot)
        {
            var button = GetButton(slot);
            if (button == null) return;
            OpenAccessoryMenu();
            button.onClick.Invoke();
        }

        private static void OpenAccessoryMenu()
        {
            if (_mainMenu == null) _mainMenu = Object.FindObjectOfType<CustomChangeMainMenu>();
            if (_mainMenu == null || _mainMenu.items == null || _mainMenu.items.Length <= AccessoryMenuIndex) return;
            var toggle = _mainMenu.items[AccessoryMenuIndex].tglItem;
            if (toggle != null && !toggle.isOn) toggle.isOn = true;
        }

        private static Button GetButton(int slot)
        {
            if (slot < 0) return null;
            if (slot < VanillaSlotCount)
            {
                var buttons = VanillaButtons();
                return buttons == null ? null : buttons[slot];
            }

            var slots = MoreAccessoriesSlots();
            var index = slot - VanillaSlotCount;
            if (slots == null || index >= slots.Count) return null;
            return Traverse.Create(slots[index]).Field("slotButton").GetValue<Button>();
        }

        private static Button[] VanillaButtons()
        {
            if (_vanillaButtons != null) return _vanillaButtons;
            var go = GameObject.Find("CharaCustom/CustomControl/CanvasMain/SubMenu/SubMenuAccessory");
            var window = go == null ? null : go.GetComponent<CvsSelectWindow>();
            if (window == null || window.items == null) return null;

            var buttons = new Button[VanillaSlotCount];
            foreach (var item in window.items)
            {
                if (item == null || !(item.cvsBase is CvsA_Slot) || item.No < 0 || item.No >= VanillaSlotCount) continue;
                buttons[item.No] = item.btnItem;
            }
            _vanillaButtons = buttons;
            return buttons;
        }

        private static IList MoreAccessoriesSlots()
        {
            if (!_moreAccessoriesSearched)
            {
                _moreAccessoriesSearched = true;
                var type = AccessTools.TypeByName("MoreAccessoriesAI.MoreAccessories");
                _moreAccessories = type == null ? null : AccessTools.Field(type, "_self")?.GetValue(null);
            }
            return _moreAccessories == null ? null : Traverse.Create(_moreAccessories).Field("_makerSlots").GetValue<IList>();
        }
    }
}
