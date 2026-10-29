using ChaCustom;
using KKAPI.Maker;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AdvancedAccessoriesMenu
{
    internal static class MakerSlots
    {
        private const int AccessoryMenuIndex = 4;

        private static CustomChangeMainMenu _mainMenu;
        private static CustomAcsChangeSlot _changeSlot;

        public static void Reset()
        {
            _mainMenu = null;
            _changeSlot = null;
        }

        public static string Name(int slot)
        {
            var toggle = GetToggle(slot);
            var text = toggle == null ? null : toggle.GetComponentInChildren<TextMeshProUGUI>();
            return text == null ? null : text.text;
        }

        public static void Select(int slot)
        {
            var toggle = GetToggle(slot);
            if (toggle == null) return;
            OpenAccessoryMenu();
            toggle.isOn = true;
        }

        private static void OpenAccessoryMenu()
        {
            if (_mainMenu == null) _mainMenu = Object.FindObjectOfType<CustomChangeMainMenu>();
            if (_mainMenu == null || _mainMenu.items == null || _mainMenu.items.Length <= AccessoryMenuIndex) return;
            var toggle = _mainMenu.items[AccessoryMenuIndex].tglItem;
            if (toggle != null && !toggle.isOn) toggle.isOn = true;
        }

        private static Toggle GetToggle(int slot)
        {
            if (slot < 0 || slot >= AccessoriesApi.GetMakerAccessoryCount()) return null;
            if (_changeSlot == null) _changeSlot = Object.FindObjectOfType<CustomAcsChangeSlot>();
            if (_changeSlot == null || _changeSlot.items == null || slot >= _changeSlot.items.Length) return null;
            var item = _changeSlot.items[slot];
            return item == null ? null : item.tglItem;
        }
    }
}
