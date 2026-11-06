using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SproutOfDebt.Laboratory.Editor
{
    public static class BowlLiquidBuilder
    {
        private const string LiquidSpritePath = "Assets/Art/Laboratory/lab_mixing_bowl_liquid.png";
        private const string RecipesFolder = "Assets/Data/Recipes";
        private const string BowlObjectName = "lab_mixing_bowl";
        private const string LiquidObjectName = "lab_mixing_bowl_liquid";

        [MenuItem("Tools/Laboratory/Build Bowl Liquid")]
        public static void Build()
        {
            BrewingStation station = Object.FindAnyObjectByType<BrewingStation>();
            GameObject bowl = GameObject.Find(BowlObjectName);
            if (station == null || bowl == null || !bowl.TryGetComponent(out SpriteRenderer bowlRenderer))
            {
                Debug.LogError("[BowlLiquidBuilder] Open a Laboratory scene with a BrewingStation and " + BowlObjectName);
                return;
            }

            ConfigureSpriteImport();
            AssignRecipeColors();

            Transform old = bowl.transform.Find(LiquidObjectName);
            if (old != null)
            {
                Object.DestroyImmediate(old.gameObject);
            }

            GameObject liquidObject = new GameObject(LiquidObjectName, typeof(SpriteRenderer));
            liquidObject.transform.SetParent(bowl.transform, false);
            SpriteRenderer liquid = liquidObject.GetComponent<SpriteRenderer>();
            liquid.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(LiquidSpritePath);
            liquid.sortingLayerID = bowlRenderer.sortingLayerID;
            liquid.sortingOrder = bowlRenderer.sortingOrder + 1;
            liquid.color = new Color(1f, 1f, 1f, 0f);

            BowlLiquid component = liquidObject.AddComponent<BowlLiquid>();
            SerializedObject so = new SerializedObject(component);
            so.FindProperty("station").objectReferenceValue = station;
            so.FindProperty("liquid").objectReferenceValue = liquid;
            so.ApplyModifiedProperties();

            EditorSceneManager.MarkSceneDirty(station.gameObject.scene);
            EditorSceneManager.SaveScene(station.gameObject.scene);
            Debug.Log("[BowlLiquidBuilder] Bowl liquid built");
        }

        private static void ConfigureSpriteImport()
        {
            TextureImporter importer = AssetImporter.GetAtPath(LiquidSpritePath) as TextureImporter;
            if (importer == null)
            {
                return;
            }
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100f;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
        }

        private static void AssignRecipeColors()
        {
            foreach (string guid in AssetDatabase.FindAssets("t:RecipeDefinition", new[] { RecipesFolder }))
            {
                RecipeDefinition recipe = AssetDatabase.LoadAssetAtPath<RecipeDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                if (recipe == null || recipe.Potion == null)
                {
                    continue;
                }

                SerializedObject so = new SerializedObject(recipe);
                SerializedProperty color = so.FindProperty("liquidColor");
                if (color.colorValue != Color.white)
                {
                    continue;
                }
                color.colorValue = DefaultColorFor(recipe.Potion.Id);
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(recipe);
            }
            AssetDatabase.SaveAssets();
        }

        private static Color DefaultColorFor(string potionId)
        {
            switch (potionId)
            {
                case "potion_calming":
                    return new Color(0.62f, 0.42f, 0.86f);
                case "potion_moonlight":
                    return new Color(0.5f, 0.74f, 0.96f);
                case "potion_shadow":
                    return new Color(0.28f, 0.2f, 0.5f);
                case "potion_ember":
                    return new Color(0.95f, 0.4f, 0.15f);
                case "potion_mandrake":
                    return new Color(0.45f, 0.66f, 0.26f);
                default:
                    return Color.white;
            }
        }
    }
}
