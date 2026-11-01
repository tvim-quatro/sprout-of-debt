using System.Collections;
using DrugCrafter.Core;
using UnityEngine;
using TMPro;

namespace DrugCrafter.Shop.UI
{
    /// <summary>Coin balance shown at the top of the screen.</summary>
    public class CoinCounterView : MonoBehaviour
    {
        [SerializeField] private TMP_Text amountText;
        [SerializeField] private RectTransform punchTarget;

        private IWallet wallet;

        private void Start()
        {
            wallet = GameServices.Wallet;
            if (wallet == null) return;
            wallet.CoinsChanged += OnCoinsChanged;
            amountText.text = wallet.Coins.ToString();
        }

        private void OnDestroy()
        {
            if (wallet != null) wallet.CoinsChanged -= OnCoinsChanged;
        }

        private void OnCoinsChanged(int coins)
        {
            amountText.text = coins.ToString();
            if (isActiveAndEnabled && punchTarget != null)
            {
                StopAllCoroutines();
                StartCoroutine(Punch());
            }
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
