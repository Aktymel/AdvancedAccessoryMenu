using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using KKAPI;
using KKAPI.Chara;
using KKAPI.Maker;

namespace AdvancedAccessoriesMenu
{
    [BepInPlugin(GUID, PluginDisplayName, Version)]
    [BepInDependency(KoikatuAPI.GUID, KoikatuAPI.VersionConst)]
    [BepInDependency("com.joan6694.illusionplugins.moreaccessories", BepInDependency.DependencyFlags.SoftDependency)]
#if HS2
    [BepInProcess("HoneySelect2")]
#elif AI
    [BepInProcess("AI-Syoujyo")]
    [BepInProcess("AI-Shoujo")]
#elif KKS
    [BepInProcess("KoikatsuSunshine")]
#elif KK
    [BepInProcess("Koikatu")]
    [BepInProcess("Koikatsu Party")]
#endif
    public class AdvancedAccessoriesMenuPlugin : BaseUnityPlugin
    {
        public const string GUID = "com.aktymel.advancedaccessoriesmenu";
        public const string PluginDisplayName = "Advanced Accessories Menu";
        public const string Version = "0.1.0";

        internal static new ManualLogSource Logger;

        internal ConfigEntry<bool> HideEmptySlots;
        private AccessoriesMenuWindow _window;

        private void Awake()
        {
            Logger = base.Logger;
            HideEmptySlots = Config.Bind("General", "Hide empty slots", false, "Leave empty accessory slots out of the Accessories menu.");
            _window = new AccessoriesMenuWindow(this);

            var harmony = Harmony.CreateAndPatchAll(typeof(Hooks), GUID);
            try
            {
                Hooks.PatchMoreAccessories(harmony);
            }
            catch (Exception e)
            {
                // A MoreAccessories version this wasn't made for only loses the slot name fix
                Logger.LogWarning("Could not patch MoreAccessories: " + e);
            }
            CharacterApi.RegisterExtraBehaviour<GroupsController>(GUID);
            MakerAPI.MakerFinishedLoading += (sender, e) =>
            {
                MakerSlots.Reset();
                NativeMenu.Reset();
            };
            MakerAPI.MakerExiting += (sender, e) =>
            {
                _window.Reset();
                MakerSlots.Reset();
                NativeMenu.Reset();
                AccessoryHighlight.Reset();
            };
        }

        private bool WindowShown => _window.Visible && NativeMenu.Shown;

        private void Update()
        {
            if (!MakerAPI.InsideAndLoaded) return;
            NativeMenu.SetListHidden(WindowShown);
            if (!WindowShown) AccessoryHighlight.Show(-1);
        }

        private void OnGUI()
        {
            if (!MakerAPI.InsideAndLoaded || !NativeMenu.Shown) return;
            if (NativeMenu.DrawToggleButton(_window.Visible)) _window.Visible = !_window.Visible;
            if (_window.Visible) _window.OnGUI(NativeMenu.ListRect);
        }
    }
}
