using System.Collections.Generic;
using KKAPI.Maker;
using KKAPI.Utilities;
using UnityEngine;
#if HS2 || AI
using AIChara;
#endif

namespace AdvancedAccessoriesMenu
{
    internal sealed class AccessoriesMenuWindow
    {
        private const int WindowId = 0x41414D31;
        // Slots the game has without MoreAccessories
        private const int VanillaSlotCount = 20;
        private static readonly Color SelectedColor = new Color(0.4f, 0.8f, 1f);

        private const float RowHeight = 24;
        private const float ButtonHeight = 22;

        // A group header (Slot is -1) or a slot; Group is null for "Ungrouped"
        private struct Row
        {
            public SlotGroup Group;
            public int Slot;
        }

        private readonly AdvancedAccessoriesMenuPlugin _plugin;
        private readonly List<Row> _rows = new List<Row>();
        private int _firstRow;
        private int _endRow;
        private float _viewHeight = 600;
        private int _hoveredSlot = -1;
        private Vector2 _scroll;
        private bool _ungroupedCollapsed;

        // Group editor; _editing is null while adding a new group. _editorGroups is the list it works on, and
        // _suggestFirst asks for the first free slot to be filled in for the next new group.
        private List<SlotGroup> _editorGroups;
        private bool _suggestFirst = true;
        private SlotGroup _editing;
        private bool _confirmDelete;
        private string _name = "";
        private string _first = "";
        private string _last = "";
        private string _error;

        private GUIStyle _headerStyle;
        private GUIStyle _rowButtonStyle;
        private GUIStyle _groupStyle;
        private GUIStyle _slotStyle;
        private GUIStyle _warningStyle;

        public bool Visible;

        public AccessoriesMenuWindow(AdvancedAccessoriesMenuPlugin plugin)
        {
            _plugin = plugin;
        }

        public void Reset()
        {
            Visible = false;
            ClearEditor();
        }

        // Drawn over the game's slot list, so it can't be dragged or resized
        public void OnGUI(Rect rect)
        {
            if (!Visible) return;

            var skin = GUI.skin;
            GUI.skin = IMGUIUtils.SolidBackgroundGuiSkin;
            if (_headerStyle == null) _headerStyle = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold };
            if (_rowButtonStyle == null)
            {
                // Rows have a fixed height, so buttons in them keep only a thin vertical margin
                var margin = GUI.skin.button.margin;
                _rowButtonStyle = new GUIStyle(GUI.skin.button) { margin = new RectOffset(margin.left, margin.right, 1, 1) };
                _groupStyle = new GUIStyle(_rowButtonStyle) { alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Bold, wordWrap = false, clipping = TextClipping.Clip };
                _slotStyle = new GUIStyle(_rowButtonStyle) { alignment = TextAnchor.MiddleLeft, wordWrap = false, clipping = TextClipping.Clip };
            }
            if (_warningStyle == null)
            {
                _warningStyle = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, wordWrap = true };
                _warningStyle.normal.textColor = new Color(1f, 0.85f, 0.2f);
            }
            GUILayout.Window(WindowId, rect, DrawWindow, "Accessories menu", GUILayout.Width(rect.width), GUILayout.Height(rect.height));
            IMGUIUtils.EatInputInRect(rect);
            GUI.skin = skin;
        }

        private void DrawWindow(int id)
        {
            _plugin.HideEmptySlots.Value = GUILayout.Toggle(_plugin.HideEmptySlots.Value, " Hide empty slots");

            var chaCtrl = MakerAPI.GetCharacterControl();
            var controller = GroupsController.Get(chaCtrl);
            if (controller == null)
            {
                GUILayout.Label("No character.");
                return;
            }

            var groups = controller.Groups;
            DrawList(groups, chaCtrl);
            DrawNativeButtons();
            DrawEditor(groups);
        }

        // With hundreds of slots only the rows in view are drawn; the rest is empty space of the same height.
        // The rows are built in the Layout event and reused by the other events of the frame, so they always match.
        private void DrawList(List<SlotGroup> groups, ChaControl chaCtrl)
        {
            var type = Event.current.type;
            if (type == EventType.Layout) BuildRows(groups, chaCtrl);
            if (type == EventType.Repaint) _hoveredSlot = -1;

            // Long names are clipped instead of widening the list, so there's no horizontal scrollbar
            _scroll = GUILayout.BeginScrollView(_scroll, false, false, GUIStyle.none, GUI.skin.verticalScrollbar);
            if (type == EventType.Layout)
            {
                _firstRow = Mathf.Clamp((int)(_scroll.y / RowHeight), 0, _rows.Count);
                _endRow = Mathf.Clamp(_firstRow + (int)(_viewHeight / RowHeight) + 2, _firstRow, _rows.Count);
            }
            GUILayout.Space(_firstRow * RowHeight);
            for (var i = _firstRow; i < _endRow; i++)
                DrawRow(_rows[i]);
            GUILayout.Space((_rows.Count - _endRow) * RowHeight);
            GUILayout.EndScrollView();

            if (type == EventType.Repaint)
            {
                var view = GUILayoutUtility.GetLastRect();
                _viewHeight = view.height;
                // Rows at the edges reach out of the view, so the mouse must also be inside it
                if (!view.Contains(Event.current.mousePosition)) _hoveredSlot = -1;
                AccessoryHighlight.Show(_hoveredSlot);
            }
        }

        private void BuildRows(List<SlotGroup> groups, ChaControl chaCtrl)
        {
            _rows.Clear();
            var count = AccessoriesApi.GetMakerAccessoryCount();
            var hideEmpty = _plugin.HideEmptySlots.Value;
            var selected = AccessoriesApi.SelectedMakerAccSlot;

            foreach (var group in groups)
            {
                _rows.Add(new Row { Group = group, Slot = -1 });
                if (group.Collapsed) continue;
                for (var slot = group.First; slot <= group.Last && slot < count; slot++)
                    if (!hideEmpty || slot == selected || !IsEmpty(chaCtrl, slot)) _rows.Add(new Row { Group = group, Slot = slot });
            }

            // Groups are sorted, so one pass finds the slots between them
            var ungroupedHeader = _rows.Count;
            var anyUngrouped = false;
            var next = 0;
            for (var slot = 0; slot < count; slot++)
            {
                while (next < groups.Count && groups[next].Last < slot) next++;
                if (next < groups.Count && groups[next].First <= slot) continue;
                anyUngrouped = true;
                if (!_ungroupedCollapsed && (!hideEmpty || slot == selected || !IsEmpty(chaCtrl, slot))) _rows.Add(new Row { Slot = slot });
            }
            if (anyUngrouped) _rows.Insert(ungroupedHeader, new Row { Slot = -1 });
        }

        private static bool IsEmpty(ChaControl chaCtrl, int slot)
        {
            return AccessoriesApi.GetAccessoryObject(chaCtrl, slot) == null;
        }

        private void DrawNativeButtons()
        {
            GUILayout.BeginHorizontal();
            if (NativeMenu.HidesCopy && GUILayout.Button("Copy")) NativeMenu.OpenCopy();
            if (NativeMenu.CanAddSlots)
            {
                if (GUILayout.Button("+1", GUILayout.Width(50))) NativeMenu.AddSlots(false);
                if (GUILayout.Button("+10", GUILayout.Width(50))) NativeMenu.AddSlots(true);
            }
            else if (!AccessoriesApi.MoreAccessoriesInstalled)
            {
                GUILayout.Label("Missing MoreAccessories plugin", _warningStyle);
            }
            GUILayout.EndHorizontal();
        }

        // Every row has the same height, so the rows in view can be found from the scroll position
        private void DrawRow(Row row)
        {
            GUILayout.BeginHorizontal(GUIStyle.none, GUILayout.Height(RowHeight));
            if (row.Slot < 0) DrawGroupHeader(row.Group);
            else DrawSlot(row.Slot);
            GUILayout.EndHorizontal();
        }

        // Clicking the header collapses or expands the group; Group is null for "Ungrouped"
        private void DrawGroupHeader(SlotGroup group)
        {
            var collapsed = group == null ? _ungroupedCollapsed : group.Collapsed;
            var text = group == null ? "Ungrouped" : $"{group.Name}  ({group.First + 1}–{group.Last + 1})";
            if (GUILayout.Button((collapsed ? "+  " : "-  ") + text, _groupStyle, GUILayout.MinWidth(0), GUILayout.Height(ButtonHeight)))
            {
                if (group == null) _ungroupedCollapsed = !_ungroupedCollapsed;
                else group.Collapsed = !group.Collapsed;
            }
            if (group != null && GUILayout.Button("Edit", _rowButtonStyle, GUILayout.Width(40), GUILayout.Height(ButtonHeight))) StartEdit(group);
        }

        private void DrawSlot(int slot)
        {
            GUILayout.Space(24);
            var color = GUI.backgroundColor;
            if (slot == AccessoriesApi.SelectedMakerAccSlot) GUI.backgroundColor = SelectedColor;
            if (GUILayout.Button($"{slot + 1}.  {MakerSlots.Name(slot) ?? ""}", _slotStyle, GUILayout.MinWidth(0), GUILayout.Height(ButtonHeight))) MakerSlots.Select(slot);
            if (Event.current.type == EventType.Repaint && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition)) _hoveredSlot = slot;
            GUI.backgroundColor = color;
        }

        private void DrawEditor(List<SlotGroup> groups)
        {
            // Another character or outfit
            if (groups != _editorGroups)
            {
                ClearEditor();
                _editorGroups = groups;
            }
            if (_suggestFirst && _editing == null)
            {
                _first = (SlotGroups.FirstFreeSlot(groups) + 1).ToString();
                _suggestFirst = false;
            }

            GUILayout.Label(_editing == null ? "New group" : "Edit group", _headerStyle);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Name", GUILayout.Width(44));
            _name = GUILayout.TextField(_name);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Slots", GUILayout.Width(44));
            _first = GUILayout.TextField(_first, GUILayout.Width(50));
            GUILayout.Label("–", GUILayout.Width(14));
            _last = GUILayout.TextField(_last, GUILayout.Width(50));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            if (_error != null) GUILayout.Label(_error, _warningStyle);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(_editing == null ? "Add group" : "Save", GUILayout.Height(26), GUILayout.MinWidth(0))) Apply(groups);
            if (_editing != null)
            {
                if (GUILayout.Button(_confirmDelete ? "Sure?" : "Delete", GUILayout.Height(26), GUILayout.Width(56)))
                {
                    if (_confirmDelete) DeleteEdited(groups);
                    else _confirmDelete = true;
                }
                if (GUILayout.Button("Cancel", GUILayout.Height(26), GUILayout.Width(56))) ClearEditor();
            }
            GUILayout.EndHorizontal();
        }

        private void DeleteEdited(List<SlotGroup> groups)
        {
            groups.Remove(_editing);
            ClearEditor();
        }

        private void StartEdit(SlotGroup group)
        {
            _editing = group;
            _name = group.Name;
            _first = (group.First + 1).ToString();
            _last = (group.Last + 1).ToString();
            _error = null;
            _confirmDelete = false;
        }

        private void ClearEditor()
        {
            _editing = null;
            _confirmDelete = false;
            _name = "";
            _first = "";
            _last = "";
            _error = null;
            _suggestFirst = true;
        }

        private void Apply(List<SlotGroup> groups)
        {
            int first, last;
            if (!int.TryParse(_first.Trim(), out first) || !int.TryParse(_last.Trim(), out last))
            {
                _error = "Enter the first and the last slot number.";
                return;
            }
            if (first < 1 || last < first)
            {
                _error = "The first slot must be 1 or more and not after the last slot.";
                return;
            }
            if (!AccessoriesApi.MoreAccessoriesInstalled && last > VanillaSlotCount)
            {
                _error = $"Missing MoreAccessories, max slots {VanillaSlotCount}.";
                return;
            }
            first--;
            last--;

            var other = SlotGroups.FindOverlap(groups, first, last, _editing);
            if (other != null)
            {
                _error = $"Slots {first + 1}–{last + 1} overlap the group \"{other.Name}\" ({other.First + 1}–{other.Last + 1}).";
                return;
            }

            var name = SlotGroups.CleanName(_name);
            if (name.Length == 0) name = "Group " + (groups.Count + (_editing == null ? 1 : 0));

            if (_editing == null)
            {
                groups.Add(new SlotGroup { Name = name, First = first, Last = last });
            }
            else
            {
                _editing.Name = name;
                _editing.First = first;
                _editing.Last = last;
            }
            SlotGroups.Sort(groups);
            ClearEditor();
        }
    }
}
