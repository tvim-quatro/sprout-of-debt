using SproutOfDebt.UI;
using UnityEditor;
using UnityEngine;

namespace SproutOfDebt.UI.Editor
{
    /// <summary>
    /// Makes the themed arrow from Figma ("UI Kit / Cursors") the project's default cursor, so it replaces the system
    /// cursor, and builds Control_GameCursor: the scaling cursor (GameCursorView) that every scene builder places once.
    /// Change the cursor here; all screens pick it up from the prefab.
    /// </summary>
    public static class GameCursorSetup
    {
        public const string MenuPath = "Tools/UI/Build Game Cursor Prefab";
        public const string PrefabPath = "Assets/UI/Prefabs/Controls/Control_GameCursor.prefab";
        private const string CursorsFolder = "Assets/UI/Art/Cursors/";
        private static readonly string[] CursorNames = { "ui_cursor_default", "ui_cursor_hover", "ui_cursor_grab" };
        // The arrow tip is the top-left pixel of the default cursor; the hands point with their center.
        private static readonly Vector2 DefaultHotspot = Vector2.zero;
        private static readonly Vector2 HandHotspot = new Vector2(16f, 16f);

        [MenuItem(MenuPath)]
        public static void BuildPrefab()
        {
            SetUpCursorImages();
            var go = new GameObject("GameCursor");
            var view = go.AddComponent<GameCursorView>();
            var so = new SerializedObject(view);
            SetStyle(so.FindProperty("defaultCursor"), CursorNames[0], DefaultHotspot);
            SetStyle(so.FindProperty("hoverCursor"), CursorNames[1], HandHotspot);
            SetStyle(so.FindProperty("grabCursor"), CursorNames[2], HandHotspot);
            so.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(go, PrefabPath);
            Object.DestroyImmediate(go);
            Debug.Log("[GameCursorSetup] Saved " + PrefabPath);
        }

        /// <summary>Places the game cursor in the open scene, building the prefab first if it doesn't exist yet.</summary>
        public static GameObject PlaceInScene()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) == null)
            {
                BuildPrefab();
            }
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            return (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        }

        // The arrow is also the project's default cursor, shown before the scene's cursor starts.
        private static void SetUpCursorImages()
        {
            foreach (string cursorName in CursorNames)
            {
                MakeReadable(CursorsFolder + cursorName + ".png");
            }

            var arrow = AssetDatabase.LoadAssetAtPath<Texture2D>(CursorsFolder + CursorNames[0] + ".png");
            if (arrow == null)
            {
                Debug.LogError("[GameCursorSetup] Default cursor not found in " + CursorsFolder);
                return;
            }
            PlayerSettings.defaultCursor = arrow;
            PlayerSettings.cursorHotspot = DefaultHotspot;
            AssetDatabase.SaveAssets();
        }

        private static void SetStyle(SerializedProperty style, string cursorName, Vector2 hotspot)
        {
            style.FindPropertyRelative("texture").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<Texture2D>(CursorsFolder + cursorName + ".png");
            style.FindPropertyRelative("hotspot").vector2Value = hotspot;
        }

        // Cursor.SetCursor and the scaling cursor need to read the pixels. The images stay sprites, so UI code can still show them.
        private static void MakeReadable(string path)
        {
            if (AssetImporter.GetAtPath(path) is TextureImporter importer && !importer.isReadable)
            {
                importer.isReadable = true;
                importer.SaveAndReimport();
            }
        }
    }
}
