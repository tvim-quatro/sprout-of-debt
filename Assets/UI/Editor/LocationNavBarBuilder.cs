using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;

namespace SproutOfDebt.UI.Editor
{
    /// <summary>
    /// Editor helper to build and wire the shared location navigation bar.
    /// </summary>
    public static class LocationNavBarBuilder
    {
        private const string NavBtnPrefabPath = "Assets/UI/Prefabs/Buttons/Button_Nav.prefab";
        private const string FontPath = "Assets/UI/Fonts/PixelifySans_FontAsset.asset";
        private static readonly Color Cream = new Color(0.98f, 0.93f, 0.82f);

        public static LocationNavBar BuildNavBar(RectTransform parent)
        {
            var navGo = new GameObject("LocationNavBar", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LocationNavBar));
            navGo.transform.SetParent(parent, false);

            var navRt = navGo.GetComponent<RectTransform>();
            navRt.anchorMin = new Vector2(0.5f, 0f);
            navRt.anchorMax = new Vector2(0.5f, 0f);
            navRt.pivot = new Vector2(0.5f, 0f);
            navRt.sizeDelta = new Vector2(740, 52);
            navRt.anchoredPosition = new Vector2(0, 24);

            var layout = navGo.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 16;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            var navBar = navGo.GetComponent<LocationNavBar>();
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);

            var gardenBtn = CreateNavButton(navRt, "Button_Nav_Garden", "GARDEN", new Vector2(160, 48), font);
            var labBtn = CreateNavButton(navRt, "Button_Nav_Laboratory", "LABORATORY", new Vector2(180, 48), font);
            var counterBtn = CreateNavButton(navRt, "Button_Nav_Counter", "COUNTER", new Vector2(160, 48), font);
            var menuBtn = CreateNavButton(navRt, "Button_Nav_Menu", "MAIN MENU", new Vector2(150, 48), font);

            UnityEventTools.AddPersistentListener(gardenBtn.onClick, navBar.OnGardenClicked);
            UnityEventTools.AddPersistentListener(labBtn.onClick, navBar.OnLaboratoryClicked);
            UnityEventTools.AddPersistentListener(counterBtn.onClick, navBar.OnCounterClicked);
            UnityEventTools.AddPersistentListener(menuBtn.onClick, navBar.OnMenuClicked);

            var so = new SerializedObject(navBar);
            so.FindProperty("gardenButton").objectReferenceValue = gardenBtn;
            so.FindProperty("laboratoryButton").objectReferenceValue = labBtn;
            so.FindProperty("counterButton").objectReferenceValue = counterBtn;
            so.FindProperty("menuButton").objectReferenceValue = menuBtn;
            so.ApplyModifiedProperties();

            return navBar;
        }

        private static Button CreateNavButton(RectTransform parent, string name, string label, Vector2 size, TMP_FontAsset font)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(NavBtnPrefabPath);
            GameObject go;
            if (prefab != null)
            {
                go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            }
            else
            {
                go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
                go.transform.SetParent(parent, false);
            }

            go.name = name;
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = size;

            var text = go.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = label;
                text.fontSize = 22;
                text.color = Cream;
                if (font != null) text.font = font;
            }

            return go.GetComponent<Button>();
        }
    }
}
