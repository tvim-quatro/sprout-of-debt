using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DrugCrafter.Shop.UI
{
    /// <summary>One seed in the Seed Shop: icon, price, owned count and Buy button, or a lock with its milestone.</summary>
    public class SeedCardView : MonoBehaviour
    {
        [SerializeField] private Image card;
        [SerializeField] private Sprite cardNormal;
        [SerializeField] private Sprite cardLocked;
        [SerializeField] private Image icon;
        [SerializeField] private GameObject lockIcon;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text ownedText;
        [SerializeField] private GameObject priceTag;
        [SerializeField] private TMP_Text priceText;
        [SerializeField] private Button buyButton;
        [SerializeField] private TMP_Text lockedText;

        public SeedData Seed { get; private set; }

        public void Bind(SeedData seed, Action<SeedData> onBuy)
        {
            Seed = seed;
            icon.sprite = seed.icon;
            nameText.text = seed.displayName;
            priceText.text = seed.buyPrice.ToString();
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(() => onBuy(seed));
        }

        public void Refresh(bool unlocked, int unlockThreshold, bool canAfford, int owned)
        {
            card.sprite = unlocked ? cardNormal : cardLocked;
            icon.color = unlocked ? Color.white : new Color(0.35f, 0.35f, 0.35f, 1f);
            lockIcon.SetActive(!unlocked);
            priceTag.SetActive(unlocked);
            buyButton.gameObject.SetActive(unlocked);
            buyButton.interactable = canAfford;
            ownedText.gameObject.SetActive(unlocked);
            ownedText.text = $"Owned: {owned}";
            lockedText.gameObject.SetActive(!unlocked);
            lockedText.text = $"Repay {unlockThreshold}\nto unlock";
        }
    }
}
