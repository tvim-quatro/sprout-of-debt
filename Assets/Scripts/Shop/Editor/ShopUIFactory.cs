using SproutOfDebt.UI.Editor;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace SproutOfDebt.Shop.Editor
{
    /// <summary>Shared paths, colors and uGUI helpers for the shop editor builders (scene, HUD prefab, windows).</summary>
    internal static class ShopUIFactory
    {
        internal const string ArtRoot = "Assets/Art";
        internal const string UIArtRoot = "Assets/UI/Art";
        internal const string ButtonsRoot = "Assets/UI/Prefabs/Buttons";

        internal static readonly Color Cream = new Color(0.98f, 0.93f, 0.82f);
        internal static readonly Color Brown = new Color(0.24f, 0.15f, 0.09f);

        /// <summary>Font for all generated texts. Loaded lazily so it survives scene changes.</summary>
        internal static TMP_FontAsset Font => font != null ? font : (font = UIPrefabBuilder.EnsureFontAsset());

        private static TMP_FontAsset font;

        internal static readonly Vector2 Center = new Vector2(0.5f, 0.5f);

        /// <summary>Price plate with a coin icon. Returns the amount text (its parent's parent is the plate).</summary>
        internal static TMP_Text PriceTag(string name, RectTransform parent, string value, Vector2 size)
        {
            var plate = MakeImage(name, parent, UIArtRoot + "/Panels/ui_price_tag.png", false);
            plate.rectTransform.sizeDelta = size;
            var content = MakeRect("Content", plate.rectTransform);
            Stretch(content);
            float iconSize = size.y * 0.62f;
            var coin = MakeImage("CoinIcon", content, UIArtRoot + "/Icons/ui_icon_coin.png", false);
            Place(coin.rectTransform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(size.y * 0.25f, 0), new Vector2(iconSize, iconSize));
            var text = MakeText("Amount", content, value, Mathf.Round(size.y * 0.45f), Cream, TextAlignmentOptions.Center);
            var textRt = text.rectTransform;
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(size.y * 0.25f + iconSize, 0);
            textRt.offsetMax = new Vector2(-10, 0);
            return text;
        }

        internal static Button MakeButton(string prefabName, Transform parent, string name, string label, Vector2 size, float fontSize)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{ButtonsRoot}/{prefabName}.prefab");
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            go.name = name;
            ((RectTransform)go.transform).sizeDelta = size;
            var text = go.GetComponentInChildren<TMP_Text>();
            if (text != null && label != null)
            {
                text.text = label;
                text.fontSize = fontSize;
            }
            return go.GetComponent<Button>();
        }

        internal static RectTransform MakeRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        internal static Image MakeImage(string name, Transform parent, string spritePath, bool nativeSize = true)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.sprite = LoadSprite(spritePath);
            image.raycastTarget = false;
            if (nativeSize && image.sprite != null) image.SetNativeSize();
            return image;
        }

        internal static TMP_Text MakeText(string name, Transform parent, string value, float size, Color color, TextAlignmentOptions alignment)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var text = go.GetComponent<TextMeshProUGUI>();
            if (Font != null) text.font = Font;
            text.text = value;
            text.fontSize = size;
            text.color = color;
            text.alignment = alignment;
            text.raycastTarget = false;
            return text;
        }

        internal static void Stretch(RectTransform rt, float insetX = 0, float insetY = 0)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(insetX, insetY);
            rt.offsetMax = new Vector2(-insetX, -insetY);
        }

        internal static void Place(RectTransform rt, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2? size = null)
        {
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = pivot;
            if (size.HasValue) rt.sizeDelta = size.Value;
            rt.anchoredPosition = position;
        }

        internal static Sprite LoadSprite(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null) Debug.LogWarning("[ShopUI] Sprite not found: " + path);
            return sprite;
        }

        internal static void Set(Object target, string field, Object value)
        {
            var so = new SerializedObject(target);
            var property = so.FindProperty(field);
            if (property == null)
            {
                Debug.LogError($"[ShopUI] {target.GetType().Name} has no field '{field}'");
                return;
            }
            property.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        internal static void SetBool(Object target, string field, bool value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(field).boolValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        internal static void SetArray(Object target, string field, Object[] values)
        {
            var so = new SerializedObject(target);
            var property = so.FindProperty(field);
            property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
