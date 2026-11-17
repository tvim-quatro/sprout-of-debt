using UnityEditor;
using UnityEngine;

namespace SproutOfDebt.Garden.Editor
{
    /// <summary>
    /// Creates the draggable garden tools of the Garden scene from the Figma "Garden" frames:
    /// the rake, the watering can and the seed packet that seeds dragged out of the inventory become.
    /// Called by GardenSceneBuilder.
    /// </summary>
    public static class GardenToolsBuilder
    {
        private const string GardenArtPath = "Assets/Art/Garden/";
        private const string WorldLayer = "World";
        private const int ToolOrder = 30;
        // Sorting order of falling seeds and drops: just above the held tool (ToolOrder + the held boost).
        private const int PourOrder = 51;

        private static readonly Vector2 RakeCenter = new Vector2(407f, 684f);
        // Middle of the rake teeth, measured from the bottom of the rake sprite, in world units.
        private const float RakeTeethHeight = 0.2f;

        // Figma "Watering can (resting)": 211.2 x 153.6 px, the 264 x 192 sprite at 0.8 scale.
        private static readonly Vector2 WateringCanCenter = new Vector2(282.6f, 902.8f);
        private const float WateringCanScale = 0.8f;
        // Figma "Watering can (held, tilted)": rotated 25 degrees clockwise.
        private const float WateringCanTilt = -25f;
        // Points on the 264 x 192 can sprite, in pixels from its top-left corner:
        // the top of the handle (where the Figma grab cursor holds it) and the spout opening.
        private static readonly Vector2 CanHandleTop = new Vector2(145f, 14f);
        private static readonly Vector2 CanSpout = new Vector2(245f, 45f);
        // How far below the spout the water lands on the cell, in world units.
        private const float WaterFallHeight = 0.7f;

        // The seed packet is hidden until seeds are dragged out of the inventory; it only rests here (the bed center).
        private static readonly Vector2 SeedPacketRestCenter = new Vector2(1194.5f, 693f);
        // How far below the top edge of the packet sprite its mouth is, in world units.
        private const float PacketMouthDepth = 0.15f;

        public static void CreateTools(PlantCatalog catalog, GardenDragController input)
        {
            var tools = new GameObject("Tools").transform;
            CreateRake(tools);
            CreateWateringCan(tools);
            var packet = CreateSeedPacket(tools, catalog);

            var seedHandler = input.gameObject.AddComponent<GardenSeedHandler>();
            var so = new SerializedObject(seedHandler);
            so.FindProperty("catalog").objectReferenceValue = catalog;
            so.FindProperty("packet").objectReferenceValue = packet;
            so.FindProperty("dragController").objectReferenceValue = input;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateRake(Transform tools)
        {
            var renderer = GardenSceneBuilder.CreateSprite("Rake", tools, GardenArtPath + "garden_rake.png", RakeCenter, WorldLayer, ToolOrder);
            renderer.gameObject.AddComponent<BoxCollider2D>();
            var rake = renderer.gameObject.AddComponent<Rake>();

            // The teeth are at the bottom of the sprite; they till the cell under the cursor.
            float teethY = renderer.sprite != null ? -renderer.sprite.bounds.extents.y + RakeTeethHeight : 0f;
            var workPoint = CreatePoint("WorkPoint", renderer.transform, new Vector2(0f, teethY));

            var so = new SerializedObject(rake);
            so.FindProperty("workPoint").objectReferenceValue = workPoint;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateWateringCan(Transform tools)
        {
            var renderer = GardenSceneBuilder.CreateSprite("WateringCan", tools, GardenArtPath + "garden_watering_can.png",
                WateringCanCenter, WorldLayer, ToolOrder);
            renderer.transform.localScale = Vector3.one * WateringCanScale;
            renderer.gameObject.AddComponent<BoxCollider2D>();
            var can = renderer.gameObject.AddComponent<WateringCan>();

            var holdPoint = CreatePoint("HoldPoint", renderer.transform, SpritePixelToLocal(renderer.sprite, CanHandleTop));
            Vector2 spoutLocal = SpritePixelToLocal(renderer.sprite, CanSpout);
            var spout = CreatePoint("Spout", renderer.transform, spoutLocal);

            // The can is carried tilted, so the landing point is found in the tilted, scaled can and
            // stored back in its own (unrotated, unscaled) coordinates.
            Quaternion tilt = Quaternion.Euler(0f, 0f, WateringCanTilt);
            Vector2 spoutWorld = tilt * (spoutLocal * WateringCanScale);
            Vector2 landingWorld = spoutWorld + Vector2.down * WaterFallHeight;
            Vector2 landingLocal = (Vector2)(Quaternion.Inverse(tilt) * landingWorld) / WateringCanScale;
            var workPoint = CreatePoint("WorkPoint", renderer.transform, landingLocal);

            var waterEffect = GardenEffectsBuilder.CreateWaterEffect(spout, PourOrder, WaterFallHeight);

            var so = new SerializedObject(can);
            so.FindProperty("workPoint").objectReferenceValue = workPoint;
            so.FindProperty("holdPoint").objectReferenceValue = holdPoint;
            so.FindProperty("heldTilt").floatValue = WateringCanTilt;
            so.FindProperty("pourEffect").objectReferenceValue = waterEffect;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // One packet for every kind of seed: it takes the packet sprite of the plant being sown (all are the same size).
        private static SeedPacket CreateSeedPacket(Transform tools, PlantCatalog catalog)
        {
            var renderer = GardenSceneBuilder.CreateSprite("SeedPacket", tools, null, SeedPacketRestCenter, WorldLayer, ToolOrder);
            renderer.sprite = catalog.Plants.Count > 0 ? catalog.Plants[0].SeedSprite : null;
            renderer.gameObject.AddComponent<BoxCollider2D>();
            var packet = renderer.gameObject.AddComponent<SeedPacket>();

            float mouthY = renderer.sprite != null ? renderer.sprite.bounds.extents.y - PacketMouthDepth : 0f;
            var mouth = CreatePoint("Mouth", renderer.transform, new Vector2(0f, mouthY));
            var pourEffect = GardenEffectsBuilder.CreatePourEffect(mouth, PourOrder);

            var so = new SerializedObject(packet);
            so.FindProperty("workPoint").objectReferenceValue = mouth;
            // The packet is carried upright; SeedPacket tips it over only while pouring.
            so.FindProperty("heldTilt").floatValue = 0f;
            so.FindProperty("pourEffect").objectReferenceValue = pourEffect;
            so.ApplyModifiedPropertiesWithoutUndo();

            renderer.gameObject.SetActive(false);
            return packet;
        }

        private static Transform CreatePoint(string name, Transform parent, Vector2 localPosition)
        {
            var point = new GameObject(name).transform;
            point.SetParent(parent, false);
            point.localPosition = localPosition;
            return point;
        }

        // A pixel of a centered sprite (from its top-left corner) in the sprite's local units.
        private static Vector2 SpritePixelToLocal(Sprite sprite, Vector2 pixel)
        {
            if (sprite == null)
            {
                return Vector2.zero;
            }
            Vector2 size = sprite.rect.size;
            return new Vector2(pixel.x - size.x / 2f, size.y / 2f - pixel.y) / sprite.pixelsPerUnit;
        }
    }
}
