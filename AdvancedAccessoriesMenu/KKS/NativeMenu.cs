using System.Collections.Generic;
using ChaCustom;
using KKAPI.Maker;
using UnityEngine;
using UnityEngine.UI;

namespace AdvancedAccessoriesMenu
{
    // A slot's settings page is part of its entry in the slot list, so only the slot buttons are hidden (with the
    // list's scrollbar and MoreAccessories' "+1" / "+10"); the settings pages, "Copy" and "Transfer" stay
    internal static class NativeMenu
    {
        private const string SlotButtonName = "imgOff";
        private const string ScrollViewName = "Slots";
        private const string AddButtonsPath = "Slots/Viewport/Content/Add Buttons Group/";
        private const float ButtonWidth = 110;
        private const float Gap = 6;

        private static CustomAcsChangeSlot _changeSlot;
        private static readonly HashSet<CanvasGroup> HiddenGroups = new HashSet<CanvasGroup>();
        private static int _hiddenSlotCount = -1;

        public static bool HidesCopy => false;

        public static void Reset()
        {
            SetListHidden(false);
            _changeSlot = null;
        }

        public static bool Shown => ChangeSlot != null && ScreenUi.IsShown(ChangeSlot) && SlotButton(0) != null;

        public static Rect ListRect
        {
            get
            {
                var first = SlotButton(0);
                if (first == null) return new Rect();
                var firstRect = ScreenUi.ScreenRect(first);

                // MoreAccessories puts the slots into a scroll view; without it the list ends at the last slot
                var scroll = ChangeSlot.transform.Find(ScrollViewName) as RectTransform;
                var last = SlotButton(AccessoriesApi.GetMakerAccessoryCount() - 1);
                var top = scroll != null ? ScreenUi.ScreenRect(scroll).y : firstRect.y;
                var bottom = scroll != null ? ScreenUi.ScreenRect(scroll).yMax : last != null ? ScreenUi.ScreenRect(last).yMax : firstRect.yMax;

                // Up to the settings page next to the slot buttons
                var right = firstRect.xMax;
                var page = ChangeSlot.items[0].cgItem;
                if (page != null) right = Mathf.Max(right, ScreenUi.ScreenRect((RectTransform)page.transform).xMin - Gap * ScreenUi.Scale);

                return new Rect(firstRect.x, top, right - firstRect.x, bottom - top);
            }
        }

        // Right of the "Copy" button
        public static bool DrawToggleButton(bool advancedOpen)
        {
            var items = ChangeSlot.items;
            var copy = items.Length < 2 || items[items.Length - 2].tglItem == null ? null : items[items.Length - 2].tglItem.transform.Find(SlotButtonName) as RectTransform;
            if (copy == null) return false;
            var copyRect = ScreenUi.ScreenRect(copy);
            var scale = ScreenUi.Scale;
            var rect = new Rect(copyRect.xMax + Gap * scale, copyRect.y, ButtonWidth * scale, copyRect.height);
            return ScreenUi.DrawToggleButton(rect, advancedOpen);
        }

        public static void SetListHidden(bool hidden)
        {
            if (!hidden)
            {
                foreach (var group in HiddenGroups)
                    if (group != null) ScreenUi.Hide(group, false);
                HiddenGroups.Clear();
                _hiddenSlotCount = -1;
                return;
            }

            // MoreAccessories can add slots while the list is hidden
            var count = AccessoriesApi.GetMakerAccessoryCount();
            if (count == _hiddenSlotCount || ChangeSlot == null) return;
            _hiddenSlotCount = count;

            var items = ChangeSlot.items;
            for (var slot = 0; slot < count && slot < items.Length; slot++)
            {
                var toggle = items[slot].tglItem;
                if (toggle == null) continue;
                var page = items[slot].cgItem;
                foreach (Transform child in toggle.transform)
                    if (page == null || !page.transform.IsChildOf(child)) Hide(child);
            }

            var scroll = ChangeSlot.transform.Find(ScrollViewName);
            var scrollRect = scroll == null ? null : scroll.GetComponent<ScrollRect>();
            if (scrollRect != null && scrollRect.verticalScrollbar != null) Hide(scrollRect.verticalScrollbar.transform);
            var addButtons = ChangeSlot.transform.Find(AddButtonsPath.TrimEnd('/'));
            if (addButtons != null) Hide(addButtons);
        }

        public static void OpenCopy() { }

        public static bool CanAddSlots => AddButton(false) != null;

        public static void AddSlots(bool ten)
        {
            var button = AddButton(ten);
            if (button != null) button.onClick.Invoke();
        }

        private static Button AddButton(bool ten)
        {
            if (ChangeSlot == null) return null;
            var t = ChangeSlot.transform.Find(AddButtonsPath + (ten ? "Add Ten Button" : "Add One Button"));
            return t == null ? null : t.GetComponent<Button>();
        }

        private static void Hide(Transform t)
        {
            var group = ScreenUi.GetOrAddCanvasGroup(t.gameObject);
            ScreenUi.Hide(group, true);
            HiddenGroups.Add(group);
        }

        private static RectTransform SlotButton(int slot)
        {
            if (ChangeSlot == null || ChangeSlot.items == null || slot < 0 || slot >= ChangeSlot.items.Length) return null;
            var toggle = ChangeSlot.items[slot].tglItem;
            return toggle == null ? null : toggle.transform.Find(SlotButtonName) as RectTransform;
        }

        private static CustomAcsChangeSlot ChangeSlot
        {
            get
            {
                if (_changeSlot == null) _changeSlot = Object.FindObjectOfType<CustomAcsChangeSlot>();
                return _changeSlot;
            }
        }
    }
}
