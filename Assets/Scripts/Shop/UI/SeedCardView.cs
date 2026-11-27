using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace SproutOfDebt.Shop.UI
{
    /// <summary>
    /// One seed in the Seed Shop (Figma component "Seed card"): the card with the seed packet and its price plate.
    /// States: normal, hover, selected (hover frame + check), locked (dim packet + lock). "Can afford = no" fades the price.
    /// Only shows the state and forwards clicks and hover; <see cref="SeedShopWindow"/> decides what they do.
    /// </summary>
    public class SeedCardView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [Header("Card")]
        [SerializeField] private Image frame;
        [SerializeField] private Image icon;
        [SerializeField] private GameObject lockIcon;
        [SerializeField] private GameObject selectedCheck;

        [Header("Price")]
        [SerializeField] private TMP_Text priceText;
        [Tooltip("Coin and amount on the price plate; faded when the player can't buy the seed.")]
        [SerializeField] private CanvasGroup priceValue;

        [Header("Frame sprites (Figma \"Card frame\")")]
        [SerializeField] private Sprite frameNormal;
        [SerializeField] private Sprite frameHover;
        [SerializeField] private Sprite frameLocked;
        [Tooltip("Frame size in the normal and locked states; hover and selected use the bigger hover sprite.")]
        [SerializeField] private Vector2 smallFrameSize = new Vector2(138f, 171f);
        [SerializeField] private Vector2 bigFrameSize = new Vector2(150f, 183f);

        [Header("Opacity (Figma)")]
        [SerializeField, Range(0f, 1f)] private float lockedIconAlpha = 0.35f;
        [SerializeField, Range(0f, 1f)] private float cannotAffordAlpha = 0.45f;

        private Action<SeedCardView> clicked;
        private Action<SeedCardView, bool> hoverChanged;
        private bool unlocked;
        private bool selected;
        private bool hovered;

        public SeedData Seed { get; private set; }

        /// <summary>World position of the seed packet's center, where a bought packet starts flying from.</summary>
        public Vector3 IconCenter => icon.rectTransform.TransformPoint(icon.rectTransform.rect.center);

        public void Bind(SeedData seed, Action<SeedCardView> onClicked, Action<SeedCardView, bool> onHoverChanged)
        {
            Seed = seed;
            clicked = onClicked;
            hoverChanged = onHoverChanged;
            icon.sprite = seed.icon;
            priceText.text = seed.buyPrice.ToString();
        }

        public void Refresh(bool isUnlocked, bool isSelected, bool canAfford)
        {
            unlocked = isUnlocked;
            selected = isUnlocked && isSelected;
            lockIcon.SetActive(!unlocked);
            selectedCheck.SetActive(selected);
            Color tint = icon.color;
            tint.a = unlocked ? 1f : lockedIconAlpha;
            icon.color = tint;
            priceValue.alpha = unlocked && canAfford ? 1f : cannotAffordAlpha;
            UpdateFrame();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            hovered = true;
            UpdateFrame();
            hoverChanged?.Invoke(this, true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            hovered = false;
            UpdateFrame();
            hoverChanged?.Invoke(this, false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                clicked?.Invoke(this);
            }
        }

        private void OnDisable()
        {
            hovered = false;
        }

        // Locked cards keep the locked frame on hover; only the tooltip tells why.
        private void UpdateFrame()
        {
            bool big = unlocked && (selected || hovered);
            frame.sprite = !unlocked ? frameLocked : big ? frameHover : frameNormal;
            frame.rectTransform.sizeDelta = big ? bigFrameSize : smallFrameSize;
        }
    }
}
