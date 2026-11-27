using System.Collections;
using SproutOfDebt.Core;
using UnityEngine;
using TMPro;

namespace SproutOfDebt.Shop.UI
{
    /// <summary>
    /// Coin balance shown at the top of the screen. The plate keeps its designed width for short amounts
    /// and grows to the right when the amount needs more room, so the number always stays on one line.
    /// </summary>
    public class CoinCounterView : MonoBehaviour
    {
        [SerializeField] private TMP_Text amountText;
        [SerializeField] private RectTransform punchTarget;
        [Tooltip("Plate that grows with the amount. The amount text must stretch inside it.")]
        [SerializeField] private RectTransform plate;

        private IWallet wallet;
        private float minPlateWidth;
        private float textPadding;

        private void Awake()
        {
            amountText.textWrappingMode = TextWrappingModes.NoWrap;
            if (plate == null) return;
            // Measured once from the built layout: the designed width and the room taken by the coin icon and margins.
            minPlateWidth = plate.rect.width;
            textPadding = minPlateWidth - amountText.rectTransform.rect.width;
        }

        private void Start()
        {
            wallet = GameServices.Wallet;
            if (wallet == null) return;
            wallet.CoinsChanged += OnCoinsChanged;
            ShowAmount(wallet.Coins);
        }

        private void OnDestroy()
        {
            if (wallet != null) wallet.CoinsChanged -= OnCoinsChanged;
        }

        private void OnCoinsChanged(int coins)
        {
            ShowAmount(coins);
            if (isActiveAndEnabled && punchTarget != null)
            {
                StopAllCoroutines();
                StartCoroutine(Punch());
            }
        }

        private void ShowAmount(int coins)
        {
            amountText.text = coins.ToString();
            if (plate == null) return;

            float textWidth = amountText.GetPreferredValues(amountText.text).x;
            float width = Mathf.Max(minPlateWidth, Mathf.Ceil(textWidth + textPadding));
            plate.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        }

        private IEnumerator Punch()
        {
            const float duration = 0.2f;
            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                punchTarget.localScale = Vector3.one * Mathf.Lerp(1.15f, 1f, t / duration);
                yield return null;
            }
            punchTarget.localScale = Vector3.one;
        }
    }
}
