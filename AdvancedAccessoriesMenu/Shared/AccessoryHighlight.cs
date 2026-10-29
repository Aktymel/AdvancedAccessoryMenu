using System;
using System.Reflection;
using BepInEx.Configuration;
using HarmonyLib;

namespace AdvancedAccessoriesMenu
{
    // Uses SliderHighlight's own accessory highlight, so hovering a slot looks the same as in the game's list.
    // Does nothing when SliderHighlight is not installed or its accessory highlights are turned off.
    internal static class AccessoryHighlight
    {
        private static bool _searched;
        private static MethodInfo _set;
        private static MethodInfo _clear;
        private static FieldInfo _material;
        private static FieldInfo _enabled;
        private static int _slot = -1;

        public static void Reset()
        {
            _slot = -1;
        }

        public static void Show(int slot)
        {
            if (slot == _slot || !Available) return;
            try
            {
                if (slot < 0) _clear.Invoke(null, null);
                else if (Enabled) _set.Invoke(null, new object[] { slot });
            }
            catch (Exception e)
            {
                AdvancedAccessoriesMenuPlugin.Logger.LogWarning("SliderHighlight accessory highlight failed: " + e);
                _set = null;
            }
            _slot = slot;
        }

        private static bool Available
        {
            get
            {
                if (!_searched)
                {
                    _searched = true;
                    var type = AccessTools.TypeByName("SliderHighlight.SliderHighlightPlugin");
                    if (type != null)
                    {
                        _set = AccessTools.Method(type, "SetAccessoryHighlight", new[] { typeof(int) });
                        _clear = AccessTools.Method(type, "ClearAccessoryHighlight");
                        _material = AccessTools.Field(type, "_matSolid");
                        _enabled = AccessTools.Field(type, "_enabledAccessory");
                    }
                }
                return _set != null && _clear != null && _material != null && _enabled != null;
            }
        }

        private static bool Enabled
        {
            get
            {
                var enabled = _enabled.GetValue(null) as ConfigEntry<bool>;
                return _material.GetValue(null) != null && enabled != null && enabled.Value;
            }
        }
    }
}
