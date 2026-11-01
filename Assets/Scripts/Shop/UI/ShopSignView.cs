using UnityEngine;
using TMPro;

namespace DrugCrafter.Shop.UI
{
    /// <summary>Hanging sign in the shop: "OPEN" during a wave, a countdown to the next visitors during a break.</summary>
    public class ShopSignView : MonoBehaviour
    {
        [SerializeField] private TMP_Text text;

        private void Update()
        {
            var waves = CustomerWaves.Current;
            if (!waves.IsOnBreak)
            {
                text.text = "OPEN";
                return;
            }

            int seconds = Mathf.CeilToInt(waves.BreakSecondsLeft);
            text.text = $"BREAK\n{seconds / 60}:{seconds % 60:00}";
        }
    }
}
