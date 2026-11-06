using SproutOfDebt.Core.Stubs;
using SproutOfDebt.Items;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SproutOfDebt.Laboratory.Editor
{
    public static class DebugJarsBuilder
    {
        private const string TemplateName = "lab_jar_lavender";
        private const string IngredientsFolder = "Assets/Data/Ingredients";
        private const string IconsFolder = "Assets/Art/Items/Ingredients";
        private const float StartX = 2f;
        private const float Spacing = 1.2f;
        private const float RowY = -4.6f;
        private const int StartingCount = 99;

        private static readonly string[] DebugIngredientIds = { "nightshade", "emberroot", "mandrake" };

        [MenuItem("Tools/Laboratory/Add Debug Ingredient Jars")]
        public static void Build()
        {
            GameObject template = GameObject.Find(TemplateName);
            StubServicesInstaller installer = Object.FindAnyObjectByType<StubServicesInstaller>();
            if (template == null || installer == null)
            {
                Debug.LogError("[DebugJarsBuilder] Open Lab_Sandbox with " + TemplateName + " and a StubServicesInstaller");
                return;
            }

            for (int i = 0; i < DebugIngredientIds.Length; i++)
            {
                CreateJar(template, DebugIngredientIds[i], new Vector3(StartX + Spacing * i, RowY, template.transform.position.z));
                EnsureStartingItem(installer, DebugIngredientIds[i]);
            }

            EditorSceneManager.MarkSceneDirty(template.scene);
            EditorSceneManager.SaveScene(template.scene);
            Debug.Log("[DebugJarsBuilder] Debug jars added");
        }

        private static void CreateJar(GameObject template, string ingredientId, Vector3 position)
        {
            string objectName = "lab_jar_" + ingredientId + "_debug";
            GameObject existing = GameObject.Find(objectName);
            if (existing != null)
            {
                Object.DestroyImmediate(existing);
            }

            GameObject jar = Object.Instantiate(template, position, Quaternion.identity);
            jar.name = objectName;
            jar.GetComponent<SpriteRenderer>().sprite = AssetDatabase.LoadAssetAtPath<Sprite>(IconsFolder + "/icon_ingr_" + ingredientId + ".png");

            SerializedObject so = new SerializedObject(jar.GetComponent<IngredientJar>());
            so.FindProperty("ingredient").objectReferenceValue = FindIngredient(ingredientId);
            so.ApplyModifiedProperties();
        }

        private static IngredientData FindIngredient(string ingredientId)
        {
            foreach (string guid in AssetDatabase.FindAssets("t:IngredientData", new[] { IngredientsFolder }))
            {
                IngredientData data = AssetDatabase.LoadAssetAtPath<IngredientData>(AssetDatabase.GUIDToAssetPath(guid));
                if (data != null && data.id == ingredientId)
                {
                    return data;
                }
            }
            return null;
        }

        private static void EnsureStartingItem(StubServicesInstaller installer, string ingredientId)
        {
            SerializedObject so = new SerializedObject(installer);
            SerializedProperty items = so.FindProperty("startingItems");
            for (int i = 0; i < items.arraySize; i++)
            {
                if (items.GetArrayElementAtIndex(i).FindPropertyRelative("itemId").stringValue == ingredientId)
                {
                    return;
                }
            }

            items.arraySize++;
            SerializedProperty added = items.GetArrayElementAtIndex(items.arraySize - 1);
            added.FindPropertyRelative("itemId").stringValue = ingredientId;
            added.FindPropertyRelative("count").intValue = StartingCount;
            so.ApplyModifiedProperties();
        }
    }
}
