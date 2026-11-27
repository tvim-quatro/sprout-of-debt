using SproutOfDebt.Core;
using SproutOfDebt.Items;
using UnityEngine;

namespace SproutOfDebt.Laboratory.UI
{
    /// <summary>
    /// When a potion is brewed: the "Potion brewed!" toast, and the potion flies from the bowl into the inventory
    /// (Figma "Laboratory - Potion is ready": "Potion (flying to inventory)" and the "+1" badge under the bag).
    /// </summary>
    public class BrewingToast : MonoBehaviour
    {
        [SerializeField] private BrewingStation station;
        [SerializeField] private string message = "Potion brewed!";
        [Tooltip("Where the brewed potion starts flying from (the mixing bowl). Empty: the station itself.")]
        [SerializeField] private Transform flySource;

        private Camera mainCamera;

        private void Awake()
        {
            mainCamera = Camera.main;
        }

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
            FlyToInventory(station.LastBrewed);
        }

        private void FlyToInventory(PotionDefinition potion)
        {
            Transform source = flySource != null ? flySource : station.transform;
            if (potion == null || potion.Icon == null || GameServices.ItemFly == null || mainCamera == null)
            {
                return;
            }
            Vector2 screenPoint = mainCamera.WorldToScreenPoint(source.position);
            GameServices.ItemFly.FlyToInventory(potion.Icon, 1, screenPoint);
        }
    }
}
