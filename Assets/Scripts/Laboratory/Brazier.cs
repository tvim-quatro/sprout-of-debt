using UnityEngine;

namespace SproutOfDebt.Laboratory
{
    public class Brazier : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [Tooltip("The coals before they are lit and after the brew is done.")]
        [SerializeField] private Sprite unlitSprite;
        [Tooltip("The burnt coals shown while the fire burns.")]
        [SerializeField] private Sprite burntSprite;
        [SerializeField] private GameObject fire;

        public bool IsLit { get; private set; }

        private void Start()
        {
            if (!IsLit)
            {
                Extinguish();
            }
        }

        public void Light()
        {
            IsLit = true;
            ShowState(burntSprite, true);
        }

        public void Extinguish()
        {
            IsLit = false;
            ShowState(unlitSprite, false);
        }

        private void ShowState(Sprite sprite, bool fireVisible)
        {
            if (spriteRenderer != null && sprite != null)
            {
                spriteRenderer.sprite = sprite;
            }
            if (fire != null)
            {
                fire.SetActive(fireVisible);
            }
        }
    }
}
