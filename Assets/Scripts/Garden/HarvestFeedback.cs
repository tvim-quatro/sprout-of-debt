using System.Collections;
using SproutOfDebt.Core;
using SproutOfDebt.Items;
using UnityEngine;

namespace SproutOfDebt.Garden
{
    /// <summary>
    /// Figma "Garden - Harvested": the picked ingredient pops up out of the plant, tilting a little, then flies into
    /// the inventory (the shared HUD effect, or fades away where there is none), while a toast says what went into
    /// the inventory ("+3 Lavender"). Only shows the harvest; GardenPlot owns the rules.
    /// </summary>
    [RequireComponent(typeof(GardenPlot))]
    public class HarvestFeedback : MonoBehaviour
    {
        [Tooltip("Sprite that shows the picked ingredient; hidden while there is nothing to show.")]
        [SerializeField] private SpriteRenderer iconRenderer;

        [Header("Motion (world units from the cell center)")]
        [Tooltip("Where the icon appears: the middle of the ripe plant (Figma: 70.5 px above the cell center).")]
        [SerializeField] private Vector2 startOffset = new Vector2(0f, 0.705f);
        [Tooltip("Where the icon stops (Figma \"Harvested lavender\": 4 px right of and 142.5 px above the cell center).")]
        [SerializeField] private Vector2 endOffset = new Vector2(0.04f, 1.425f);
        [Tooltip("Tilt at the end, in degrees (Figma: 6).")]
        [SerializeField] private float endTilt = 6f;

        [Header("Timing")]
        [SerializeField, Min(0.01f)] private float riseSeconds = 0.35f;
        [SerializeField, Min(0f)] private float holdSeconds = 0.4f;
        [SerializeField, Min(0.01f)] private float fadeSeconds = 0.3f;

        private GardenPlot plot;
        private int poppedAmount;

        private void Awake()
        {
            plot = GetComponent<GardenPlot>();
            Hide();
        }

        private void OnEnable()
        {
            plot.Harvested += OnHarvested;
        }

        private void OnDisable()
        {
            plot.Harvested -= OnHarvested;
            StopAllCoroutines();
            Hide();
        }

        private void OnHarvested(GardenPlot source, IngredientData item, int amount)
        {
            GameServices.Toasts?.Show($"+{amount} {item.displayName}");
            if (iconRenderer == null || item.icon == null)
            {
                return;
            }
            iconRenderer.sprite = item.icon;
            poppedAmount = amount;
            StopAllCoroutines();
            StartCoroutine(Pop());
        }

        private IEnumerator Pop()
        {
            Transform icon = iconRenderer.transform;
            Vector3 center = transform.position;
            iconRenderer.enabled = true;
            SetAlpha(1f);
            for (float time = 0f; time < riseSeconds; time += Time.deltaTime)
            {
                // Eases out: fast out of the plant, slowing down at the top.
                float t = 1f - Mathf.Pow(1f - time / riseSeconds, 2f);
                icon.SetPositionAndRotation(center + (Vector3)Vector2.Lerp(startOffset, endOffset, t), Quaternion.Euler(0f, 0f, endTilt * t));
                yield return null;
            }
            icon.SetPositionAndRotation(center + (Vector3)endOffset, Quaternion.Euler(0f, 0f, endTilt));
            yield return new WaitForSeconds(holdSeconds);
            if (TryFlyToInventory())
            {
                Hide();
                yield break;
            }
            for (float time = 0f; time < fadeSeconds; time += Time.deltaTime)
            {
                SetAlpha(1f - time / fadeSeconds);
                yield return null;
            }
            Hide();
        }

        // Hands the icon over to the HUD, which flies it from the same spot on screen to the bag.
        private bool TryFlyToInventory()
        {
            Camera camera = Camera.main;
            if (GameServices.ItemFly == null || camera == null)
            {
                return false;
            }
            Vector2 screenPoint = camera.WorldToScreenPoint(iconRenderer.transform.position);
            GameServices.ItemFly.FlyToInventory(iconRenderer.sprite, poppedAmount, screenPoint);
            return true;
        }

        private void SetAlpha(float alpha)
        {
            Color color = iconRenderer.color;
            color.a = alpha;
            iconRenderer.color = color;
        }

        private void Hide()
        {
            if (iconRenderer != null)
            {
                iconRenderer.enabled = false;
            }
        }
    }
}
