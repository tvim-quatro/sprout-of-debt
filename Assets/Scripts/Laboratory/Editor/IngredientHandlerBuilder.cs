using SproutOfDebt.Items;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SproutOfDebt.Laboratory.Editor
{
    public static class IngredientHandlerBuilder
    {
        private const string IngredientsFolder = "Assets/Data/Ingredients";
        private const string OldJarPrefix = "lab_jar_";
        private const string HeldName = "HeldIngredient";
        private const string HandlerName = "LabIngredientHandler";
        private const int HeldSortingOrder = 4;
        private const float HeldScale = 0.8f;
        private const float HeldColliderSize = 0.6f;

        [MenuItem("Tools/Laboratory/Build Ingredient Handler")]
        public static void Build()
        {
            BrewingStation station = Object.FindAnyObjectByType<BrewingStation>();
            LaboratoryDragController controller = Object.FindAnyObjectByType<LaboratoryDragController>();
            if (station == null || controller == null)
            {
                Debug.LogError("[IngredientHandlerBuilder] Open a Laboratory scene with a BrewingStation and LaboratoryDragController");
                return;
            }

            Scene scene = station.gameObject.scene;
            RemoveOld(scene);

            HeldIngredient held = CreateHeldIngredient(station);
            CreateHandler(held, controller);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[IngredientHandlerBuilder] Ingredient handler built");
        }

        private static void RemoveOld(Scene scene)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (root.name.StartsWith(OldJarPrefix) || root.name == HeldName || root.name == HandlerName)
                {
                    Object.DestroyImmediate(root);
                }
            }
        }

        private static HeldIngredient CreateHeldIngredient(BrewingStation station)
        {
            GameObject go = new GameObject(HeldName, typeof(SpriteRenderer), typeof(BoxCollider2D));
            go.transform.localScale = new Vector3(HeldScale, HeldScale, 1f);
            go.GetComponent<SpriteRenderer>().sortingOrder = HeldSortingOrder;
            go.GetComponent<BoxCollider2D>().size = new Vector2(HeldColliderSize, HeldColliderSize);
            HeldIngredient held = go.AddComponent<HeldIngredient>();

            SerializedObject so = new SerializedObject(held);
            so.FindProperty("station").objectReferenceValue = station;
            so.ApplyModifiedPropertiesWithoutUndo();

            go.SetActive(false);
            return held;
        }

        private static void CreateHandler(HeldIngredient held, LaboratoryDragController controller)
        {
            LabIngredientHandler handler = new GameObject(HandlerName).AddComponent<LabIngredientHandler>();

            string[] guids = AssetDatabase.FindAssets("t:IngredientData", new[] { IngredientsFolder });
            SerializedObject so = new SerializedObject(handler);
            SerializedProperty list = so.FindProperty("ingredients");
            list.arraySize = guids.Length;
            for (int i = 0; i < guids.Length; i++)
            {
                list.GetArrayElementAtIndex(i).objectReferenceValue =
                    AssetDatabase.LoadAssetAtPath<IngredientData>(AssetDatabase.GUIDToAssetPath(guids[i]));
            }
            so.FindProperty("heldIngredient").objectReferenceValue = held;
            so.FindProperty("dragController").objectReferenceValue = controller;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
