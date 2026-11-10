using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace SproutOfDebt.Laboratory.Editor
{
    public static class RecipeBookUIFactory
    {
        private const string FontPath = "Assets/UI/Fonts/PixelifySans_FontAsset.asset";
        private const string ButtonsRoot = "Assets/UI/Prefabs/Buttons";

        public static readonly Color Dark = new Color(0.239f, 0.149f, 0.09f);
        public static readonly Color Error = new Color(0.62f, 0.129f, 0.102f);

        private static TMP_FontAsset font;

        public static void LoadFont()
        {
            font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
        }

        public static RectTransform MakeRect(string name, Transform parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        public static Image MakeImage(string name, Transform parent, string spritePath)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            Image image = go.GetComponent<Image>();
            image.raycastTarget = false;
            image.preserveAspect = true;
            if (spritePath != null)
            {
                image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            }
            return image;
        }

        public static TMP_Text MakeText(string name, Transform parent, string value, float size, bool bold, Color color, TextAlignmentOptions alignment)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            TextMeshProUGUI text = go.GetComponent<TextMeshProUGUI>();
            if (font != null)
            {
                text.font = font;
            }
            text.text = value;
            text.fontSize = size;
            text.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
            text.lineSpacing = 30f;
            text.color = color;
            text.alignment = alignment;
            text.textWrappingMode = TextWrappingModes.Normal;
            text.raycastTarget = false;
            return text;
        }

        public static Button MakeButton(string prefabName, Transform parent, string name, string label)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ButtonsRoot + "/" + prefabName + ".prefab");
            GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            go.name = name;
            TMP_Text text = go.GetComponentInChildren<TMP_Text>();
            if (text != null && label != null)
            {
                text.text = label;
            }
            return go.GetComponent<Button>();
        }

        public static VerticalLayoutGroup Vertical(GameObject go, float spacing, TextAnchor alignment)
        {
            VerticalLayoutGroup layout = go.AddComponent<VerticalLayoutGroup>();
            layout.spacing = spacing;
            layout.childAlignment = alignment;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            return layout;
        }

        public static HorizontalLayoutGroup Horizontal(GameObject go, float spacing, TextAnchor alignment)
        {
            HorizontalLayoutGroup layout = go.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = spacing;
            layout.childAlignment = alignment;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            return layout;
        }

        public static LayoutElement Size(Component target, float width, float height)
        {
            LayoutElement element = target.gameObject.AddComponent<LayoutElement>();
            if (width >= 0f)
            {
                element.preferredWidth = width;
            }
            if (height >= 0f)
            {
                element.preferredHeight = height;
            }
            return element;
        }

        public static void Flexible(Component target, float width, float height)
        {
            LayoutElement element = target.gameObject.AddComponent<LayoutElement>();
            element.flexibleWidth = width;
            element.flexibleHeight = height;
        }

        public static void Place(RectTransform rect, Vector2 position, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
        }

        public static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        public static void Set(Object target, string field, Object value)
        {
            SerializedObject so = new SerializedObject(target);
            so.FindProperty(field).objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void SetArray(Object target, string field, Object[] values)
        {
            SerializedObject so = new SerializedObject(target);
            SerializedProperty property = so.FindProperty(field);
            property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
