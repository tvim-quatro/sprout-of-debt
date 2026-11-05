using UnityEngine;

namespace SproutOfDebt.Laboratory
{
    [RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
    public abstract class LaboratoryTool : MonoBehaviour
    {
        private const int DragOrderBoost = 10;

        [SerializeField] private BrewingStation station;

        private SpriteRenderer spriteRenderer;
        private Vector3 homePosition;
        private int homeOrder;

        protected BrewingStation Station => station;
        protected SpriteRenderer SpriteRenderer => spriteRenderer;

        public virtual bool CanPickUp => station != null;

        protected virtual void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            homePosition = transform.position;
            homeOrder = spriteRenderer.sortingOrder;
        }

        public void PickUp()
        {
            spriteRenderer.sortingOrder = homeOrder + DragOrderBoost;
        }

        public void MoveTo(Vector2 position)
        {
            transform.position = new Vector3(position.x, position.y, homePosition.z);
        }

        public void ReturnHome()
        {
            transform.position = homePosition;
            spriteRenderer.sortingOrder = homeOrder;
        }

        public abstract bool UseOn(Collider2D target, float deltaTime);
    }
}
