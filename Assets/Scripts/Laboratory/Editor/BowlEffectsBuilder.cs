using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SproutOfDebt.Laboratory.Editor
{
    public static class BowlEffectsBuilder
    {
        private const string EffectsFolder = "Assets/Art/Effects";
        private const string RootName = "BowlEffects";
        private const string BowlObjectName = "lab_mixing_bowl";
        private const int SortingOrder = 5;
        private static readonly Vector3 SurfaceOffset = new Vector3(0f, 0.55f, 0f);

        [MenuItem("Tools/Laboratory/Build Bowl Effects")]
        public static void Build()
        {
            BrewingStation station = Object.FindAnyObjectByType<BrewingStation>();
            GameObject bowl = GameObject.Find(BowlObjectName);
            if (station == null || bowl == null)
            {
                Debug.LogError("[BowlEffectsBuilder] Open a Laboratory scene with a BrewingStation and " + BowlObjectName);
                return;
            }

            GameObject oldRoot = GameObject.Find(RootName);
            if (oldRoot != null)
            {
                Object.DestroyImmediate(oldRoot);
            }

            GameObject root = new GameObject(RootName);
            root.transform.position = bowl.transform.position + SurfaceOffset;
            BowlEffects effects = root.AddComponent<BowlEffects>();

            ParticleSystem splash = CreateSystem(root.transform, "Splash", "fx_splash");
            ConfigureSplash(splash);
            ParticleSystem bubbles = CreateSystem(root.transform, "Bubbles", "fx_bubble");
            ConfigureBubbles(bubbles);
            ParticleSystem steam = CreateSystem(root.transform, "Steam", "fx_steam");
            ConfigureSteam(steam);
            ParticleSystem sparkles = CreateSystem(root.transform, "Sparkles", "fx_magic_sparkle");
            ConfigureSparkles(sparkles);

            SerializedObject so = new SerializedObject(effects);
            so.FindProperty("station").objectReferenceValue = station;
            so.FindProperty("splash").objectReferenceValue = splash;
            so.FindProperty("bubbles").objectReferenceValue = bubbles;
            so.FindProperty("steam").objectReferenceValue = steam;
            so.FindProperty("sparkles").objectReferenceValue = sparkles;
            so.ApplyModifiedProperties();

            EditorSceneManager.MarkSceneDirty(station.gameObject.scene);
            EditorSceneManager.SaveScene(station.gameObject.scene);
            Debug.Log("[BowlEffectsBuilder] Bowl effects built");
        }

        private static ParticleSystem CreateSystem(Transform parent, string name, string spriteName)
        {
            GameObject go = new GameObject(name, typeof(ParticleSystem));
            go.transform.SetParent(parent, false);
            ParticleSystem system = go.GetComponent<ParticleSystem>();
            system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            ParticleSystem.MainModule main = system.main;
            main.playOnAwake = false;
            main.loop = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.cullingMode = ParticleSystemCullingMode.AlwaysSimulate;

            ParticleSystemRenderer particleRenderer = go.GetComponent<ParticleSystemRenderer>();
            particleRenderer.sortingOrder = SortingOrder;
            particleRenderer.sharedMaterial = EnsureMaterial(spriteName);
            return system;
        }

        private static Material EnsureMaterial(string spriteName)
        {
            string path = EffectsFolder + "/" + spriteName + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null)
            {
                return material;
            }

            Shader shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
            if (shader == null)
            {
                shader = Shader.Find("Sprites/Default");
            }
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(EffectsFolder + "/" + spriteName + ".png");
            material = new Material(shader) { mainTexture = texture };
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static void ConfigureSplash(ParticleSystem system)
        {
            ParticleSystem.MainModule main = system.main;
            main.startLifetime = 0.5f;
            main.startSpeed = 0f;
            SetSize(system, 1.25f, 0.91f);
            SetBurst(system, 1);
            SetShapeOff(system);
            SetSizeOverLifetime(system, 0.8f, 1.1f);
            SetFadeOut(system);
        }

        private static void ConfigureBubbles(ParticleSystem system)
        {
            ParticleSystem.MainModule main = system.main;
            main.loop = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1.1f, 1.7f);
            main.startSpeed = 0f;
            main.startSize3D = true;
            main.startSizeX = new ParticleSystem.MinMaxCurve(0.25f, 0.4f);
            main.startSizeY = new ParticleSystem.MinMaxCurve(0.25f, 0.4f);
            main.startSizeZ = 1f;
            ParticleSystem.EmissionModule emission = system.emission;
            emission.rateOverTime = 4f;
            system.transform.localPosition = new Vector3(0f, -0.05f, 0f);
            SetBoxShape(system, new Vector3(1.1f, 0.3f, 0f));

            ParticleSystem.VelocityOverLifetimeModule velocity = system.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.World;
            velocity.x = 0f;
            velocity.y = new ParticleSystem.MinMaxCurve(0.2f, 0.45f);
            velocity.z = 0f;

            ParticleSystem.NoiseModule noise = system.noise;
            noise.enabled = true;
            noise.separateAxes = false;
            noise.strength = 0.25f;
            noise.frequency = 0.7f;
            noise.scrollSpeed = 0.5f;
            noise.damping = true;
            noise.quality = ParticleSystemNoiseQuality.Low;

            SetSizeOverLifetime(system, 0.8f, 1.15f);
            SetFadeOut(system);
        }

        private static void ConfigureSteam(ParticleSystem system)
        {
            ParticleSystem.MainModule main = system.main;
            main.startLifetime = 1.4f;
            main.startSpeed = 0f;
            SetSize(system, 0.9f, 0.96f);
            SetBurst(system, 2);
            SetBoxShape(system, new Vector3(0.8f, 0.1f, 0f));
            SetRise(system, 0.7f);
            SetSizeOverLifetime(system, 0.7f, 1.2f);
            SetFadeOut(system);
        }

        private static void ConfigureSparkles(ParticleSystem system)
        {
            ParticleSystem.MainModule main = system.main;
            main.startLifetime = 1.2f;
            main.startSpeed = 0f;
            SetSize(system, 0.4f, 0.4f);
            SetBurst(system, 4);
            system.transform.localPosition = new Vector3(0f, 1.1f, 0f);
            SetBoxShape(system, new Vector3(2.2f, 1f, 0f));
            SetRise(system, 0.3f);
            SetSizeOverLifetime(system, 1f, 0.4f);
            SetFadeOut(system);
        }

        private static void SetSize(ParticleSystem system, float width, float height)
        {
            ParticleSystem.MainModule main = system.main;
            main.startSize3D = true;
            main.startSizeX = width;
            main.startSizeY = height;
            main.startSizeZ = 1f;
        }

        private static void SetBurst(ParticleSystem system, short count)
        {
            ParticleSystem.EmissionModule emission = system.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, count) });
        }

        private static void SetShapeOff(ParticleSystem system)
        {
            ParticleSystem.ShapeModule shape = system.shape;
            shape.enabled = false;
        }

        private static void SetBoxShape(ParticleSystem system, Vector3 scale)
        {
            ParticleSystem.ShapeModule shape = system.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = scale;
        }

        private static void SetRise(ParticleSystem system, float speed)
        {
            ParticleSystem.VelocityOverLifetimeModule velocity = system.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.World;
            velocity.x = new ParticleSystem.MinMaxCurve(-0.15f, 0.15f);
            velocity.y = speed;
            velocity.z = 0f;
        }

        private static void SetSizeOverLifetime(ParticleSystem system, float from, float to)
        {
            ParticleSystem.SizeOverLifetimeModule size = system.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, from, 1f, to));
        }

        private static void SetFadeOut(ParticleSystem system)
        {
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0.9f, 0f), new GradientAlphaKey(0f, 1f) });
            ParticleSystem.ColorOverLifetimeModule color = system.colorOverLifetime;
            color.enabled = true;
            color.color = gradient;
        }
    }
}
