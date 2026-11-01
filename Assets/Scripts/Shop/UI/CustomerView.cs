using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace DrugCrafter.Shop.UI
{
    /// <summary>Customer standing behind the counter: expression sprite plus enter/leave animation.</summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class CustomerView : MonoBehaviour
    {
        [SerializeField] private Image image;
        [Tooltip("How far below its spot the customer starts when entering")]
        [SerializeField] private float slideDistance = 90f;
        [SerializeField] private float animationDuration = 0.35f;

        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;
        private Vector2 homePosition;
        private CustomerData current;

        /// <summary>World position of the middle of the customer's sprite.</summary>
        public Vector3 Center
        {
            get
            {
                Init();
                return rectTransform.TransformPoint(rectTransform.rect.center);
            }
        }

        private void Awake()
        {
            HideImmediate();
        }

        // The counter controller may call in before this component's Awake.
        private void Init()
        {
            if (rectTransform != null) return;
            rectTransform = (RectTransform)transform;
            canvasGroup = GetComponent<CanvasGroup>();
            homePosition = rectTransform.anchoredPosition;
        }

        public void HideImmediate()
        {
            Init();
            canvasGroup.alpha = 0f;
            rectTransform.anchoredPosition = homePosition;
        }

        public void SetMood(CustomerMood mood)
        {
            if (current == null) return;
            image.sprite = current.GetSprite(mood);
            image.SetNativeSize();
        }

        public IEnumerator Enter(CustomerData customer)
        {
            current = customer;
            SetMood(CustomerMood.Neutral);
            yield return Animate(-slideDistance, 0f, 0f, 1f);
        }

        public IEnumerator Leave()
        {
            yield return Animate(0f, -slideDistance, 1f, 0f);
        }

        private IEnumerator Animate(float fromOffset, float toOffset, float fromAlpha, float toAlpha)
        {
            Init();
            for (float t = 0f; t < animationDuration; t += Time.deltaTime)
            {
                float k = Mathf.SmoothStep(0f, 1f, t / animationDuration);
                rectTransform.anchoredPosition = homePosition + Vector2.up * Mathf.Lerp(fromOffset, toOffset, k);
                canvasGroup.alpha = Mathf.Lerp(fromAlpha, toAlpha, k);
                yield return null;
            }
            rectTransform.anchoredPosition = homePosition + Vector2.up * toOffset;
            canvasGroup.alpha = toAlpha;
        }
    }
}
