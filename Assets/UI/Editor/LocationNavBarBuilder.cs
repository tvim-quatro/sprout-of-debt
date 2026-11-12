using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;

namespace SproutOfDebt.UI.Editor
{
    /// <summary>
    /// Builds the shared location navigation bar prefab (Figma: "Location switcher") and places it in scenes.
    /// </summary>
    public static class LocationNavBarBuilder
    {
        private const string NavBtnPrefabPath = "Assets/UI/Prefabs/Buttons/Button_Nav.prefab";
        private const string NavBarPrefabPath = "Assets/UI/Prefabs/Bars/Control_LocationNavBar.prefab";
        private const string FontPath = "Assets/UI/Fonts/PixelifySans_FontAsset.asset";
        private const string ActiveSpritePath = "Assets/UI/Art/Buttons/ui_btn_nav_active.png";
        private const string IconsPath = "Assets/UI/Art/Icons/";

        // Sizes in reference pixels (1920x1080), taken from Figma.
        private static readonly Vector2 ButtonSize = new Vector2(312, 90);
        private const float ButtonSpacing = 16f;
        private const float BottomOffset = 24f;
        private const float IconSize = 48f;
        private const float IconLabelSpacing = 12f;
        private const float LabelFontSize = 24f;

        private static readonly Color Cream = new Color(0.98f, 0.93f, 0.82f);
        private static readonly Color Brown = new Color(0.24f, 0.15f, 0.09f);

        [MenuItem("Tools/UI/Build Location Nav Bar Prefab")]
        public static void BuildPrefab()
        {
            var root = CreateNavBar();
            Directory.CreateDirectory(Path.GetDirectoryName(NavBarPrefabPath));
            PrefabUtility.SaveAsPrefabAsset(root, NavBarPrefabPath);
            Object.DestroyImmediate(root);
            Debug.Log("[LocationNavBarBuilder] Saved " + NavBarPrefabPath);
        }

        /// <summary>Places an instance of the nav bar prefab under the given canvas, building the prefab first if needed.</summary>
        public static LocationNavBar BuildNavBar(RectTransform parent)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(NavBarPrefabPath) == null)
            {
                BuildPrefab();
            }

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(NavBarPrefabPath);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            return instance.GetComponent<LocationNavBar>();
        }

        private static GameObject CreateNavBar()
        {
            var navGo = new GameObject("LocationNavBar", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LocationNavBar));

            var navRt = navGo.GetComponent<RectTransform>();
            navRt.anchorMin = new Vector2(0.5f, 0f);
            navRt.anchorMax = new Vector2(0.5f, 0f);
            navRt.pivot = new Vector2(0.5f, 0f);
            navRt.sizeDelta = new Vector2(ButtonSize.x * 4 + ButtonSpacing * 3, ButtonSize.y);
            navRt.anchoredPosition = new Vector2(0, BottomOffset);

            var layout = navGo.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = ButtonSpacing;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            var navBar = navGo.GetComponent<LocationNavBar>();
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);

            var gardenBtn = CreateNavButton(navRt, "Button_Nav_Garden", "Garden", "ui_icon_nav_garden", font);
            var labBtn = CreateNavButton(navRt, "Button_Nav_Laboratory", "Laboratory", "ui_icon_nav_lab", font);
            var counterBtn = CreateNavButton(navRt, "Button_Nav_Counter", "Counter", "ui_icon_nav_counter", font);
            // Temporary: not in the design, stays until the Settings window can return to the menu.
            var menuBtn = CreateNavButton(navRt, "Button_Nav_Menu", "Main Menu", null, font);

            UnityEventTools.AddPersistentListener(gardenBtn.onClick, navBar.OnGardenClicked);
            UnityEventTools.AddPersistentListener(labBtn.onClick, navBar.OnLaboratoryClicked);
            UnityEventTools.AddPersistentListener(counterBtn.onClick, navBar.OnCounterClicked);
            UnityEventTools.AddPersistentListener(menuBtn.onClick, navBar.OnMenuClicked);

            var so = new SerializedObject(navBar);
            so.FindProperty("gardenButton").objectReferenceValue = gardenBtn;
            so.FindProperty("laboratoryButton").objectReferenceValue = labBtn;
            so.FindProperty("counterButton").objectReferenceValue = counterBtn;
            so.FindProperty("menuButton").objectReferenceValue = menuBtn;
            so.FindProperty("activeSprite").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(ActiveSpritePath);
            so.FindProperty("activeLabelColor").colorValue = Brown;
            so.ApplyModifiedPropertiesWithoutUndo();

            return navGo;
        }

        private static Button CreateNavButton(RectTransform parent, string name, string label, string iconName, TMP_FontAsset font)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(NavBtnPrefabPath);
            GameObject go;
            if (prefab != null)
            {
                go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            }
            else
            {
                Debug.LogWarning("[LocationNavBarBuilder] " + NavBtnPrefabPath + " not found, run Tools > UI > Generate All UI Prefabs");
                go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
                go.transform.SetParent(parent, false);
            }

            go.name = name;
            ((RectTransform)go.transform).sizeDelta = ButtonSize;

            // Icon and label sit side by side, centered on the button.
            var content = go.AddComponent<HorizontalLayoutGroup>();
            content.spacing = IconLabelSpacing;
            content.childAlignment = TextAnchor.MiddleCenter;
            content.childControlWidth = true;
            content.childControlHeight = true;
            content.childForceExpandWidth = false;
            content.childForceExpandHeight = false;

            if (!string.IsNullOrEmpty(iconName))
            {
                CreateIcon(go.transform, iconName);
            }

            var text = go.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = label;
                text.fontSize = LabelFontSize;
                text.color = Cream;
                text.textWrappingMode = TextWrappingModes.NoWrap;
                if (font != null) text.font = font;

                // Figma: dark text on the light hover face, cream text on the dark normal face.
                var hoverColor = go.AddComponent<ButtonLabelHoverColor>();
                var so = new SerializedObject(hoverColor);
                so.FindProperty("label").objectReferenceValue = text;
                so.FindProperty("normalColor").colorValue = Cream;
                so.FindProperty("hoverColor").colorValue = Brown;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            return go.GetComponent<Button>();
        }

        private static void CreateIcon(Transform button, string iconName)
        {
            var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(LayoutElement));
            iconGo.transform.SetParent(button, false);
            iconGo.transform.SetAsFirstSibling();

            var image = iconGo.GetComponent<Image>();
            image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(IconsPath + iconName + ".png");
            image.preserveAspect = true;
            image.raycastTarget = false;

            var element = iconGo.GetComponent<LayoutElement>();
            element.preferredWidth = IconSize;
            element.preferredHeight = IconSize;
        }
    }
}
