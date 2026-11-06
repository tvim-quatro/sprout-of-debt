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
        private Quaternion homeRotation;
        private int homeOrder;

        protected BrewingStation Station => station;
        protected SpriteRenderer SpriteRenderer => spriteRenderer;

        public virtual bool CanPickUp => station != null;

        protected virtual void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            homePosition = transform.position;
            homeRotation = transform.rotation;
            homeOrder = spriteRenderer.sortingOrder;
        }

        public void PickUp()
        {
            spriteRenderer.sortingOrder = homeOrder + DragOrderBoost;
            OnPickedUp();
        }

        public void MoveTo(Vector2 position)
        {
            Vector2 delta = position - (Vector2)transform.position;
            transform.position = new Vector3(position.x, position.y, homePosition.z);
            OnMoved(delta, Time.deltaTime);
        }

        public void ReturnHome()
        {
            transform.position = homePosition;
            transform.rotation = homeRotation;
            spriteRenderer.sortingOrder = homeOrder;
            OnReturnedHome();
        }

        public virtual Vector2 GetUsePoint()
        {
            return transform.position;
        }

        public abstract bool UseOn(Collider2D target, float deltaTime);

        protected virtual void OnPickedUp()
        {
        }

        protected virtual void OnMoved(Vector2 delta, float deltaTime)
        {
        }

        protected virtual void OnReturnedHome()
        {
        }
    }
}
