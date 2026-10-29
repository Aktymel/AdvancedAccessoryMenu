using KKAPI.Utilities;
using UnityEngine;

namespace AdvancedAccessoriesMenu
{
    internal static class ScreenUi
    {
        // UI sizes are made for 1080p
        public static float Scale => Screen.height / 1080f;

        // The button that switches between the game's slot list and the Accessories menu
        public static bool DrawToggleButton(Rect rect, bool advancedOpen)
        {
            var skin = GUI.skin;
            GUI.skin = IMGUIUtils.SolidBackgroundGuiSkin;
            var clicked = GUI.Button(rect, advancedOpen ? "Original" : "Advanced");
            GUI.skin = skin;
            IMGUIUtils.EatInputInRect(rect);
            return clicked;
        }

        // Maker hides its menus by CanvasGroup alpha, so activeInHierarchy alone is not enough
        public static bool IsShown(Component component)
        {
            if (component == null || !component.gameObject.activeInHierarchy) return false;
            foreach (var group in component.GetComponentsInParent<CanvasGroup>())
            {
                if (group.alpha <= 0.01f) return false;
                if (group.ignoreParentGroups) break;
            }
            return true;
        }

        // Screen area of a UI element in GUI coordinates
        public static Rect ScreenRect(RectTransform rt)
        {
            var corners = new Vector3[4];
            rt.GetWorldCorners(corners);
            var canvas = rt.GetComponentInParent<Canvas>();
            if (canvas != null) canvas = canvas.rootCanvas;
            var cam = canvas == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            var min = RectTransformUtility.WorldToScreenPoint(cam, corners[0]);
            var max = RectTransformUtility.WorldToScreenPoint(cam, corners[2]);
            return new Rect(min.x, Screen.height - max.y, max.x - min.x, max.y - min.y);
        }

        public static void Hide(CanvasGroup group, bool hidden)
        {
            group.alpha = hidden ? 0 : 1;
            group.interactable = !hidden;
            group.blocksRaycasts = !hidden;
        }

        // Also finds components on maker pages that are inactive at the moment
        public static T FindInScene<T>() where T : Component
        {
            foreach (var component in Resources.FindObjectsOfTypeAll<T>())
                if (component.gameObject.scene.IsValid()) return component;
            return null;
        }

        public static CanvasGroup GetOrAddCanvasGroup(GameObject go)
        {
            var group = go.GetComponent<CanvasGroup>();
            return group != null ? group : go.AddComponent<CanvasGroup>();
        }
    }
}
