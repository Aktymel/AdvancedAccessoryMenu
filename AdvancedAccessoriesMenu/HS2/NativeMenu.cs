using CharaCustom;
using UnityEngine;
using UnityEngine.UI;

namespace AdvancedAccessoriesMenu
{
    // The game's slot list is hidden as a whole, together with its "Copy" entry and MoreAccessories' "+1" / "+10"
    internal static class NativeMenu
    {
        private const string SelectWindowPath = "CharaCustom/CustomControl/CanvasMain/SubMenu/SubMenuAccessory";
        private const float ButtonWidth = 110;
        private const float ButtonHeight = 32;
        private const float ButtonGap = 8;

        private static CvsSelectWindow _selectWindow;
        private static RectTransform _list;
        private static CanvasGroup _listGroup;
        private static bool _hidden;

        public static bool HidesCopy => true;

        public static void Reset()
        {
            SetListHidden(false);
            _selectWindow = null;
            _list = null;
            _listGroup = null;
        }

        public static bool Shown => ScreenUi.IsShown(SelectWindow);

        public static Rect ListRect => List == null ? new Rect() : ScreenUi.ScreenRect(List);

        // Above the right end of the slot list, next to the "Accessories" title
        public static bool DrawToggleButton(bool advancedOpen)
        {
            var list = ListRect;
            var scale = ScreenUi.Scale;
            var size = new Vector2(ButtonWidth * scale, ButtonHeight * scale);
            var rect = new Rect(list.xMax - size.x, list.y - size.y - ButtonGap * scale, size.x, size.y);
            return ScreenUi.DrawToggleButton(rect, advancedOpen);
        }

        public static void SetListHidden(bool hidden)
        {
            if (hidden == _hidden || List == null) return;
            _hidden = hidden;
            if (_listGroup == null) _listGroup = ScreenUi.GetOrAddCanvasGroup(List.gameObject);
            ScreenUi.Hide(_listGroup, hidden);
        }

        public static void OpenCopy()
        {
            var window = SelectWindow;
            if (window == null || window.items == null) return;
            foreach (var item in window.items)
            {
                if (item == null || !(item.cvsBase is CvsA_Copy) || item.btnItem == null) continue;
                item.btnItem.onClick.Invoke();
                return;
            }
        }

        public static bool CanAddSlots => AddButton("AddOne") != null;

        public static void AddSlots(bool ten)
        {
            var button = AddButton(ten ? "AddTen" : "AddOne");
            if (button != null) button.onClick.Invoke();
        }

        private static Button AddButton(string name)
        {
            if (List == null) return null;
            var t = List.Find("Viewport/Content/Category/CategoryTop/AddButtons/" + name);
            return t == null ? null : t.GetComponent<Button>();
        }

        private static CvsSelectWindow SelectWindow
        {
            get
            {
                if (_selectWindow == null)
                {
                    var go = GameObject.Find(SelectWindowPath);
                    _selectWindow = go == null ? null : go.GetComponent<CvsSelectWindow>();
                }
                return _selectWindow;
            }
        }

        private static RectTransform List
        {
            get
            {
                if (_list == null && SelectWindow != null) _list = SelectWindow.transform.Find("Scroll View") as RectTransform;
                return _list;
            }
        }
    }
}
