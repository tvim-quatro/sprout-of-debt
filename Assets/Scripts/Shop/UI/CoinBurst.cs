using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SproutOfDebt.Shop.UI
{
    /// <summary>
    /// Sale reward (Figma: Counter components, "Sale reward"): coins pile up on the counter tray with sparkles,
    /// then spinning coins fly from the pile up to the coin counter in the HUD.
    /// </summary>
    public class CoinBurst : MonoBehaviour
    {
        [SerializeField] private Sprite[] spinFrames;
        [SerializeField] private Vector2 coinSize = new Vector2(46, 48);
        [SerializeField] private Image coinPile;
        [Tooltip("fx_sparkle images on the pile; they twinkle while it is shown")]
        [SerializeField] private Image[] sparkles;
        [Tooltip("Where the coins fly. Empty: the HUD coin counter is found at runtime.")]
        [SerializeField] private RectTransform hudTarget;
        [SerializeField] private int maxCoins = 8;
        [SerializeField] private float pileDelay = 0.25f;
        [SerializeField] private float flightDuration = 0.7f;
        [SerializeField] private float delayBetweenCoins = 0.08f;
        [SerializeField] private float frameDuration = 0.07f;
        [SerializeField] private float arcHeight = 120f;

        private readonly List<GameObject> flyingCoins = new List<GameObject>();

        private void Awake()
        {
            ClearPile();
        }

        private void OnDisable()
        {
            // Coroutines stop when the location is switched; don't leave coins frozen mid-air.
            DestroyFlyingCoins();
            ClearPile();
        }

        public void Play(int coinsEarned)
        {
            StopAllCoroutines();
            DestroyFlyingCoins();
            StartCoroutine(PlayRoutine(Mathf.Clamp(coinsEarned / 5, 3, maxCoins)));
        }

        public void ClearPile()
        {
            if (coinPile != null)
            {
                coinPile.rectTransform.localScale = Vector3.one;
                coinPile.gameObject.SetActive(false);
            }
        }

        private void DestroyFlyingCoins()
        {
            foreach (var coin in flyingCoins)
            {
                if (coin != null) Destroy(coin);
            }
            flyingCoins.Clear();
        }

        private IEnumerator PlayRoutine(int count)
        {
            coinPile.gameObject.SetActive(true);
            StartCoroutine(Twinkle());
            yield return Pop(coinPile.rectTransform);
            yield return new WaitForSeconds(pileDelay);

            var target = ResolveTarget();
            if (target == null) yield break;
            for (int i = 0; i < count; i++)
            {
                StartCoroutine(FlyCoin(target));
                yield return new WaitForSeconds(delayBetweenCoins);
            }
        }

        private RectTransform ResolveTarget()
        {
            if (hudTarget == null)
            {
                var counter = FindAnyObjectByType<CoinCounterView>();
                if (counter != null) hudTarget = (RectTransform)counter.transform;
            }
            return hudTarget;
        }

        private IEnumerator FlyCoin(RectTransform target)
        {
            var go = new GameObject("Coin", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var rt = (RectTransform)go.transform;
            rt.SetParent(transform, false);
            rt.sizeDelta = coinSize;
            var image = go.GetComponent<Image>();
            image.raycastTarget = false;
            flyingCoins.Add(go);

            float scale = transform.lossyScale.x;
            var pileRt = coinPile.rectTransform;
            Vector3 start = pileRt.TransformPoint(pileRt.rect.center) + (Vector3)(Random.insideUnitCircle * 40f * scale);
            Vector3 end = target.TransformPoint(target.rect.center);
            Vector3 control = (start + end) * 0.5f + (Vector3)(Random.insideUnitCircle * arcHeight * scale);
            int frame = Random.Range(0, Mathf.Max(1, spinFrames.Length));

            for (float t = 0f; t < flightDuration; t += Time.deltaTime)
            {
                float k = Mathf.SmoothStep(0f, 1f, t / flightDuration);
                rt.position = Vector3.Lerp(Vector3.Lerp(start, control, k), Vector3.Lerp(control, end, k), k);
                if (spinFrames.Length > 0)
                {
                    image.sprite = spinFrames[(frame + (int)(t / frameDuration)) % spinFrames.Length];
                }
                yield return null;
            }

            flyingCoins.Remove(go);
            Destroy(go);
        }

        private IEnumerator Twinkle()
        {
            if (sparkles == null) yield break;
            float time = 0f;
            while (coinPile.gameObject.activeSelf)
            {
                for (int i = 0; i < sparkles.Length; i++)
                {
                    if (sparkles[i] == null) continue;
                    float k = 0.5f + 0.5f * Mathf.Sin(time * 6f + i * 2.1f);
                    sparkles[i].rectTransform.localScale = Vector3.one * Mathf.Lerp(0.5f, 1.1f, k);
                }
                time += Time.deltaTime;
                yield return null;
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
