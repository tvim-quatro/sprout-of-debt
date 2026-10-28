using UnityEngine;

namespace DrugCrafter.Laboratory
{
    public class Brazier : MonoBehaviour
    {
        public SpriteRenderer spriteRenderer;
        public Sprite emptySprite;
        public Sprite litSprite;
        public GameObject fire;

        public bool IsLit { get; private set; }

        private void Start()
        {
            Extinguish();
        }

        public void Light()
        {
            IsLit = true;
            spriteRenderer.sprite = litSprite;
            fire.SetActive(true);
        }

        public void Extinguish()
        {
            IsLit = false;
            spriteRenderer.sprite = emptySprite;
            fire.SetActive(false);
        }
    }
}
