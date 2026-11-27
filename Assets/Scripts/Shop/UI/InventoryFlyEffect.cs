using System.Collections;
using System.Collections.Generic;
using SproutOfDebt.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SproutOfDebt.Shop.UI
{
    /// <summary>
    /// HUD part of the "item flies into the inventory" effect (Figma "Laboratory - Potion is ready"):
    /// the item icon arcs from where it appeared to the bag button, the button gives a small punch,
    /// and the "Added badge" with "+N" shows under the button for a moment.
    /// </summary>
    public class InventoryFlyEffect : MonoBehaviour, IItemFlyService
    {
        [Header("References")]
        [Tooltip("Layer the flying icons are created in; it covers the 16:9 frame and blocks no clicks.")]
        [SerializeField] private RectTransform layer;
        [Tooltip("Icon copied for every flight; hidden itself.")]
        [SerializeField] private Image iconTemplate;
        [Tooltip("The inventory (bag) button the items fly into.")]
        [SerializeField] private RectTransform target;
        [SerializeField] private CanvasGroup badge;
        [SerializeField] private TMP_Text badgeText;

        [Header("Flight")]
        [SerializeField, Min(0.05f)] private float flightSeconds = 0.6f;
        [Tooltip("How high the arc goes above the straight line, in frame pixels.")]
        [SerializeField] private float arcHeight = 160f;
        [SerializeField, Range(0.1f, 1f)] private float endScale = 0.55f;

        [Header("Badge")]
        [SerializeField, Min(0f)] private float badgeSeconds = 1.1f;
        [SerializeField, Min(0.01f)] private float badgeFadeSeconds = 0.25f;
        [SerializeField, Min(0.01f)] private float punchSeconds = 0.18f;
        [SerializeField] private float punchScale = 1.12f;

        private readonly List<GameObject> flying = new List<GameObject>();
        private Coroutine badgeRoutine;
        private Coroutine punchRoutine;

        private void Awake()
        {
            if (iconTemplate != null)
            {
                iconTemplate.gameObject.SetActive(false);
            }
            SetBadgeVisible(0f);
        }

        private void OnEnable()
        {
            GameServices.ItemFly = this;
        }

        private void OnDisable()
        {
            if (ReferenceEquals(GameServices.ItemFly, this))
            {
                GameServices.ItemFly = null;
            }
            if (target != null)
            {
                target.localScale = Vector3.one;
            }
            SetBadgeVisible(0f);
            // Coroutines stop with the component, so icons still in the air are removed here.
            foreach (GameObject icon in flying)
            {
                if (icon != null)
                {
                    Destroy(icon);
                }
            }
            flying.Clear();
            badgeRoutine = null;
            punchRoutine = null;
        }

        public void FlyToInventory(Sprite icon, int amount, Vector2 screenPoint)
        {
            if (layer == null || iconTemplate == null || target == null || icon == null || !isActiveAndEnabled)
            {
                return;
            }
            Image item = Instantiate(iconTemplate, layer);
            item.sprite = icon;
            item.gameObject.SetActive(true);
            flying.Add(item.gameObject);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(layer, screenPoint, null, out Vector2 start);
            StartCoroutine(Fly(item, start, amount));
        }

        private IEnumerator Fly(Image item, Vector2 start, int amount)
        {
            RectTransform rect = item.rectTransform;
            Vector2 end = layer.InverseTransformPoint(target.TransformPoint(target.rect.center));
            Vector2 control = (start + end) * 0.5f + Vector2.up * arcHeight;
            for (float time = 0f; time < flightSeconds; time += Time.unscaledDeltaTime)
            {
                // Eases in: the item lifts off slowly and speeds up towards the bag.
                float t = time / flightSeconds;
                t *= t;
                rect.anchoredPosition = Bezier(start, control, end, t);
                rect.localScale = Vector3.one * Mathf.Lerp(1f, endScale, t);
                yield return null;
            }
            flying.Remove(item.gameObject);
            Destroy(item.gameObject);
            ShowArrival(amount);
        }

        private void ShowArrival(int amount)
        {
            if (punchRoutine != null)
            {
                StopCoroutine(punchRoutine);
            }
            punchRoutine = StartCoroutine(Punch());
            if (badge == null || badgeText == null)
            {
                return;
            }
            badgeText.text = "+" + amount;
            if (badgeRoutine != null)
            {
                StopCoroutine(badgeRoutine);
            }
            badgeRoutine = StartCoroutine(ShowBadge());
        }

        private IEnumerator Punch()
        {
            for (float time = 0f; time < punchSeconds; time += Time.unscaledDeltaTime)
            {
                float t = time / punchSeconds;
                target.localScale = Vector3.one * Mathf.Lerp(punchScale, 1f, t);
                yield return null;
            }
            target.localScale = Vector3.one;
            punchRoutine = null;
        }

        private IEnumerator ShowBadge()
        {
            SetBadgeVisible(1f);
            yield return new WaitForSecondsRealtime(badgeSeconds);
            for (float time = 0f; time < badgeFadeSeconds; time += Time.unscaledDeltaTime)
            {
                SetBadgeVisible(1f - time / badgeFadeSeconds);
                yield return null;
            }
            SetBadgeVisible(0f);
            badgeRoutine = null;
        }

        private void SetBadgeVisible(float alpha)
        {
            if (badge != null)
            {
                badge.alpha = alpha;
            }
        }

        private static Vector2 Bezier(Vector2 a, Vector2 b, Vector2 c, float t)
        {
            float u = 1f - t;
            return u * u * a + 2f * u * t * b + t * t * c;
        }
    }
}
