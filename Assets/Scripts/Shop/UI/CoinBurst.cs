using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DrugCrafter.Shop.UI
{
    /// <summary>Spinning coins fly from the customer to the counter tray, where a coin pile appears.</summary>
    public class CoinBurst : MonoBehaviour
    {
        [SerializeField] private Sprite[] spinFrames;
        [SerializeField] private Vector2 coinSize = new Vector2(40, 42);
        [SerializeField] private RectTransform target;
        [SerializeField] private Image coinPile;
        [SerializeField] private int maxCoins = 10;
        [SerializeField] private float flightDuration = 0.55f;
        [SerializeField] private float delayBetweenCoins = 0.06f;
        [SerializeField] private float frameDuration = 0.07f;
        [SerializeField] private float arcHeight = 160f;

        private readonly List<GameObject> flyingCoins = new List<GameObject>();

        private void Awake()
        {
            ClearPile();
        }

        private void OnDisable()
        {
            // Coroutines stop when the location is switched; don't leave coins frozen mid-air.
            foreach (var coin in flyingCoins)
            {
                if (coin != null) Destroy(coin);
            }
            flyingCoins.Clear();
            if (coinPile != null) coinPile.rectTransform.localScale = Vector3.one;
        }

        public void Play(Vector3 from, int coinsEarned)
        {
            int count = Mathf.Clamp(coinsEarned / 5, 3, maxCoins);
            for (int i = 0; i < count; i++)
            {
                StartCoroutine(FlyCoin(from, i * delayBetweenCoins, i == count - 1));
            }
        }

        public void ClearPile()
        {
            if (coinPile != null) coinPile.gameObject.SetActive(false);
        }

        private IEnumerator FlyCoin(Vector3 from, float delay, bool isLast)
        {
            yield return new WaitForSeconds(delay);

            var go = new GameObject("Coin", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var rt = (RectTransform)go.transform;
            rt.SetParent(transform, false);
            rt.sizeDelta = coinSize;
            var image = go.GetComponent<Image>();
            image.raycastTarget = false;
            flyingCoins.Add(go);

            float scale = transform.lossyScale.x;
            Vector3 start = from + (Vector3)(Random.insideUnitCircle * 30f * scale);
            Vector3 end = target.position + (Vector3)(Random.insideUnitCircle * 20f * scale);
            Vector3 control = (start + end) * 0.5f + Vector3.up * arcHeight * scale;
            int frame = Random.Range(0, Mathf.Max(1, spinFrames.Length));

            for (float t = 0f; t < flightDuration; t += Time.deltaTime)
            {
                float k = t / flightDuration;
                rt.position = Vector3.Lerp(Vector3.Lerp(start, control, k), Vector3.Lerp(control, end, k), k);
                if (spinFrames.Length > 0)
                {
                    image.sprite = spinFrames[(frame + (int)(t / frameDuration)) % spinFrames.Length];
                }
                yield return null;
            }

            flyingCoins.Remove(go);
            Destroy(go);
            if (isLast && coinPile != null)
            {
                coinPile.gameObject.SetActive(true);
                yield return Pop(coinPile.rectTransform);
            }
        }

        private static IEnumerator Pop(RectTransform rt)
        {
            const float duration = 0.18f;
            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                rt.localScale = Vector3.one * Mathf.Lerp(1.25f, 1f, t / duration);
                yield return null;
            }
            rt.localScale = Vector3.one;
        }
    }
}
