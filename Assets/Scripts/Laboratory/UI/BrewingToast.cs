using SproutOfDebt.Core;
using UnityEngine;

namespace SproutOfDebt.Laboratory.UI
{
    public class BrewingToast : MonoBehaviour
    {
        [SerializeField] private BrewingStation station;
        [SerializeField] private string message = "Potion brewed!";

        private void OnEnable()
        {
            if (station != null)
            {
                station.PotionBrewed += OnPotionBrewed;
            }
        }

        private void OnDisable()
        {
            if (station != null)
            {
                station.PotionBrewed -= OnPotionBrewed;
            }
        }

        private void OnPotionBrewed()
        {
            if (GameServices.Toasts != null)
            {
                GameServices.Toasts.Show(message);
            }
        }
    }
}
