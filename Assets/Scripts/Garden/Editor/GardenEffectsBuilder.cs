using UnityEditor;
using UnityEngine;

namespace SproutOfDebt.Garden.Editor
{
    /// <summary>
    /// Creates the particle effects of the garden cells (same setup as the Laboratory bowl effects).
    /// Called by GardenSceneBuilder, so the effects are rebuilt together with the scene.
    /// </summary>
    public static class GardenEffectsBuilder
    {
        private const string EffectsFolder = "Assets/Art/Effects";
        // Small particle grains drawn at the game's 3x pixel scale, shown at their real size (PPU 100) like all art.
        private const string ClodSprite = "fx_clod_grain";
        private const string SeedSprite = "fx_seed_grain";
        private const float PixelsPerUnit = 100f;
        private const string SortingLayer = "World";
        // Above the soil, the bed frame and the plants, below the held tool.
        private const int TillSortingOrder = 25;

        /// <summary>A short burst of soil clods that fly up from the cell, fall and fade out in about half a second.</summary>
        public static ParticleSystem CreateTillEffect(Transform plot)
        {
            var system = CreateSystem("TillEffect", plot, ClodSprite, TillSortingOrder);
            var main = system.main;
            main.loop = false;
            main.duration = 0.5f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.55f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 3.2f);
            main.gravityModifier = 1.2f;

            var emission = system.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 6, 9) });

            // A narrow cone pointing up, so the clods fly up and a little to the sides.
            var shape = system.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 30f;
            shape.radius = 0.3f;
            shape.rotation = new Vector3(-90f, 0f, 0f);

            SetFadeOut(system);
            return system;
        }

        /// <summary>A thin stream of seeds that falls from the packet mouth while the packet is held.</summary>
        public static ParticleSystem CreatePourEffect(Transform mouth, int sortingOrder)
        {
            var system = CreateSystem("PourEffect", mouth, SeedSprite, sortingOrder);
            var main = system.main;
            main.loop = true;
            // Seeds fall about half a cell before they fade, so the stream is clearly visible.
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 0.7f);
            main.startSpeed = 0f;
            main.gravityModifier = 0.4f;

            var emission = system.emission;
            emission.rateOverTime = 14f;

            var shape = system.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.08f;

            // Seeds spread a little to the sides as they fall.
            var velocity = system.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.World;
            velocity.x = new ParticleSystem.MinMaxCurve(-0.25f, 0.25f);
            velocity.y = new ParticleSystem.MinMaxCurve(-0.9f, -0.6f);
            // All three axes must use the same curve mode (random between two constants).
            velocity.z = new ParticleSystem.MinMaxCurve(0f, 0f);

            SetFadeOut(system);
            return system;
        }

        private static ParticleSystem CreateSystem(string name, Transform parent, string spriteName, int sortingOrder)
        {
            var go = new GameObject(name, typeof(ParticleSystem));
            go.transform.SetParent(parent, false);
            var system = go.GetComponent<ParticleSystem>();
            system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = system.main;
            main.playOnAwake = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.cullingMode = ParticleSystemCullingMode.AlwaysSimulate;

            var particleRenderer = go.GetComponent<ParticleSystemRenderer>();
            particleRenderer.sortingLayerName = SortingLayer;
            particleRenderer.sortingOrder = sortingOrder;
            particleRenderer.sharedMaterial = EnsureMaterial(spriteName, out Texture2D texture);

            // Real size of the image, unrotated, so its pixels match the rest of the art at any window size.
            if (texture != null)
            {
                main.startSize3D = true;
                main.startSizeX = texture.width / PixelsPerUnit;
                main.startSizeY = texture.height / PixelsPerUnit;
                main.startSizeZ = 1f;
            }
            return system;
        }

        private static void SetFadeOut(ParticleSystem system)
        {
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.6f), new GradientAlphaKey(0f, 1f) });
            var color = system.colorOverLifetime;
            color.enabled = true;
            color.color = gradient;
        }

        private static Material EnsureMaterial(string spriteName, out Texture2D texture)
        {
            string texturePath = EffectsFolder + "/" + spriteName + ".png";
            SetPixelArtImport(texturePath);
            texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);

            string path = EffectsFolder + "/" + spriteName + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null)
            {
                return material;
            }

            var shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
            if (shader == null)
            {
                shader = Shader.Find("Sprites/Default");
            }
            material = new Material(shader) { mainTexture = texture };
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        // Pixel-art import settings from AGENTS.md: Point filter, no compression, no mip maps, PPU 100.
        private static void SetPixelArtImport(string path)
        {
            if (!(AssetImporter.GetAtPath(path) is TextureImporter importer))
            {
                return;
            }
            bool changed = importer.textureType != TextureImporterType.Sprite
                || importer.filterMode != FilterMode.Point
                || importer.textureCompression != TextureImporterCompression.Uncompressed
                || importer.mipmapEnabled
                || !Mathf.Approximately(importer.spritePixelsPerUnit, PixelsPerUnit);
            if (!changed)
            {
                return;
            }
            importer.textureType = TextureImporterType.Sprite;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.spritePixelsPerUnit = PixelsPerUnit;
            importer.SaveAndReimport();
        }
    }
}
