using System.IO;
using UnityEditor;
using UnityEngine;
using TMPro;
using static SproutOfDebt.UI.Editor.UIPrefabUtils;

namespace SproutOfDebt.UI.Editor
{
    public static class UIPrefabBuilder
    {
        [MenuItem("Tools/UI/Generate All UI Prefabs")]
        public static void BuildAllPrefabs()
        {
            EnsureDirectories();
            var fontAsset = EnsureFontAsset();

            ButtonPrefabBuilder.BuildButtons(fontAsset);
            PanelPrefabBuilder.BuildPanels(fontAsset);
            ControlPrefabBuilder.BuildControls(fontAsset);
            BarPrefabBuilder.BuildBars(fontAsset);
            CanvasPrefabBuilder.BuildSampleCanvas(fontAsset);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[UIPrefabBuilder] Successfully generated all UI Prefabs in " + PrefabsRoot);
        }

        private static void EnsureDirectories()
        {
            Directory.CreateDirectory(Path.Combine(PrefabsRoot, "Buttons"));
            Directory.CreateDirectory(Path.Combine(PrefabsRoot, "Panels"));
            Directory.CreateDirectory(Path.Combine(PrefabsRoot, "Controls"));
            Directory.CreateDirectory(Path.Combine(PrefabsRoot, "Bars"));
            Directory.CreateDirectory(Path.Combine(PrefabsRoot, "Canvases"));
        }

        public static TMP_FontAsset EnsureFontAsset()
        {
            // Use TMP default font asset (LiberationSans SDF) or any existing valid font asset
            // To create PixelifySans, use Unity Editor's Window > TextMeshPro > Font Asset Creator
            string fontAssetPath = Path.Combine(FontsRoot, "PixelifySans_FontAsset.asset").Replace("\\", "/");
            var fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontAssetPath);
            if (fontAsset != null && fontAsset.atlasTextures != null && fontAsset.atlasTextures.Length > 0 && fontAsset.atlasTextures[0] != null)
            {
                return fontAsset;
            }

            // Fallback to default TMP font asset so text never crashes
            string defaultPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";
            var defaultFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(defaultPath);
            if (defaultFont != null)
            {
                return defaultFont;
            }

            return null;
        }
    }
}
