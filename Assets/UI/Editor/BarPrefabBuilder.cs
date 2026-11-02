using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static SproutOfDebt.UI.Editor.UIPrefabUtils;

namespace SproutOfDebt.UI.Editor
{
    // Builds bar prefabs (Prefabs/Bars).
    internal static class BarPrefabBuilder
    {
        internal static void BuildBars(TMP_FontAsset font)
        {
            var go = new GameObject("Control_DebtBar", typeof(RectTransform));
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(282, 40);

            // Frame
            var frameGo = new GameObject("Frame", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            frameGo.transform.SetParent(go.transform, false);
            var frameRt = frameGo.GetComponent<RectTransform>();
            frameRt.anchorMin = Vector2.zero;
            frameRt.anchorMax = Vector2.one;
            frameRt.offsetMin = Vector2.zero;
            frameRt.offsetMax = Vector2.zero;
            var frameImg = frameGo.GetComponent<Image>();
            frameImg.sprite = LoadSprite("Panels/ui_bar_frame_9s.png");
            frameImg.type = Image.Type.Sliced;

            // Fill
            var fillGo = new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            fillGo.transform.SetParent(go.transform, false);
            var fillRt = fillGo.GetComponent<RectTransform>();
            fillRt.anchorMin = new Vector2(0, 0.15f);
            fillRt.anchorMax = new Vector2(0.7f, 0.85f);
            fillRt.offsetMin = new Vector2(6, 0);
            fillRt.offsetMax = new Vector2(-6, 0);
            var fillImg = fillGo.GetComponent<Image>();
            fillImg.sprite = LoadSprite("Bars/ui_bar_fill.png");
            fillImg.type = Image.Type.Sliced;

            SavePrefab(go, Path.Combine(PrefabsRoot, "Bars/Control_DebtBar.prefab"));
            Object.DestroyImmediate(go);
        }
    }
}
