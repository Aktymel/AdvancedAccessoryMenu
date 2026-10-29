using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdvancedAccessoriesMenu
{
    internal sealed class SlotGroup
    {
        public string Name;
        public int First;
        public int Last;
        public bool Collapsed;

        public bool Contains(int slot) => slot >= First && slot <= Last;
    }

    internal static class SlotGroups
    {
        public static SlotGroup FindOverlap(List<SlotGroup> groups, int first, int last, SlotGroup except)
        {
            return groups.FirstOrDefault(g => g != except && first <= g.Last && last >= g.First);
        }

        public static int FirstFreeSlot(List<SlotGroup> groups)
        {
            var slot = 0;
            foreach (var g in groups)
                if (g.Contains(slot)) slot = g.Last + 1;
            return slot;
        }

        public static void Sort(List<SlotGroup> groups)
        {
            groups.Sort((a, b) => a.First.CompareTo(b.First));
        }

        public static string Encode(List<SlotGroup> groups)
        {
            var sb = new StringBuilder();
            foreach (var g in groups)
                sb.Append(g.First).Append('\t').Append(g.Last).Append('\t').Append(g.Collapsed ? 1 : 0).Append('\t').Append(CleanName(g.Name)).Append('\n');
            return sb.ToString();
        }

        public static List<SlotGroup> Decode(string text)
        {
            var groups = new List<SlotGroup>();
            if (string.IsNullOrEmpty(text)) return groups;
            foreach (var line in text.Split('\n'))
            {
                var parts = line.Split(new[] { '\t' }, 4);
                if (parts.Length < 4) continue;
                int first, last;
                if (!int.TryParse(parts[0], out first) || !int.TryParse(parts[1], out last) || first < 0 || last < first) continue;
                // Skip broken entries instead of loading overlapping groups
                if (FindOverlap(groups, first, last, null) != null) continue;
                groups.Add(new SlotGroup { First = first, Last = last, Collapsed = parts[2] == "1", Name = parts[3] });
            }
            Sort(groups);
            return groups;
        }

        public static string CleanName(string name)
        {
            return (name ?? "").Replace('\t', ' ').Replace('\r', ' ').Replace('\n', ' ').Trim();
        }
    }
}
