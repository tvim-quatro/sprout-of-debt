using UnityEngine;

namespace SproutOfDebt.Laboratory
{
    public class Brazier : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Sprite emptySprite;
        [SerializeField] private Sprite litSprite;
        [SerializeField] private GameObject fire;

        public bool IsLit { get; private set; }

        private void Start()
        {
            Extinguish();
        }

        public void Light()
        {
            IsLit = true;
            ShowState(litSprite, true);
        }

        public void Extinguish()
        {
            IsLit = false;
            ShowState(emptySprite, false);
        }

        private void ShowState(Sprite sprite, bool fireVisible)
        {
            if (spriteRenderer != null)
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
