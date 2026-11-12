using UnityEngine;

namespace SproutOfDebt.Garden
{
    /// <summary>
    /// A garden item the player drags with the mouse (rake, seed packets, later the watering can).
    /// According to GDD: while held over a valid cell the item performs its action; when released it returns to its place.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
    public abstract class GardenTool : MonoBehaviour
    {
        private const int HeldOrderBoost = 20;

        [Header("Holding")]
        [Tooltip("Point that acts on the cells (the rake teeth). The cursor holds the tool at this point.")]
        [SerializeField] private Transform workPoint;
        [Tooltip("Tilt in degrees while the tool is held (Figma: the held tool leans).")]
        [SerializeField] private float heldTilt = 20f;

        private SpriteRenderer spriteRenderer;
        private Vector3 homePosition;
        private Quaternion homeRotation;
        private int homeOrder;

        /// <summary>False when the tool can't be used now (an empty seed packet).</summary>
        public virtual bool CanPickUp => true;

        protected virtual void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            homePosition = transform.position;
            homeRotation = transform.rotation;
            homeOrder = spriteRenderer.sortingOrder;
        }

        public void PickUp()
        {
            SetTilt(heldTilt);
            spriteRenderer.sortingOrder = homeOrder + HeldOrderBoost;
            OnPickedUp();
        }

        /// <summary>Moves the tool so that its work point sits under the cursor.</summary>
        public void MoveTo(Vector2 cursor)
        {
            Vector2 offset = workPoint != null ? (Vector2)(workPoint.position - transform.position) : Vector2.zero;
            Vector2 position = cursor - offset;
            transform.position = new Vector3(position.x, position.y, homePosition.z);
        }

        public void ReturnHome()
        {
            transform.SetPositionAndRotation(homePosition, homeRotation);
            spriteRenderer.sortingOrder = homeOrder;
            OnReturnedHome();
        }

        public Vector2 WorkPoint => workPoint != null ? (Vector2)workPoint.position : (Vector2)transform.position;

        /// <summary>Called every frame while held, with the cell under the work point (null when there is none).</summary>
        public virtual void Hover(GardenPlot plot)
        {
        }

        /// <summary>Acts on the cell under the work point. Returns true if the cell changed.</summary>
        public abstract bool UseOn(GardenPlot plot);

        protected void SetTilt(float degrees)
        {
            transform.rotation = Quaternion.Euler(0f, 0f, degrees);
        }

        protected virtual void OnPickedUp()
        {
        }

        protected virtual void OnReturnedHome()
        {
        }
    }
}
