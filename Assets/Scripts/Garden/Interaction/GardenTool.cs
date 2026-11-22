using UnityEngine;

namespace SproutOfDebt.Garden
{
    /// <summary>
    /// A garden item the player drags with the mouse (rake, watering can, seed packets).
    /// According to GDD: while held over a valid cell the item performs its action; when released it returns to its place.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
    public abstract class GardenTool : MonoBehaviour
    {
        private const int HeldOrderBoost = 20;

        [Header("Holding")]
        [Tooltip("Point that acts on the cells (the rake teeth, where the water lands).")]
        [SerializeField] private Transform workPoint;
        [Tooltip("Point the cursor holds (the can handle). Empty = the cursor holds the tool at its work point.")]
        [SerializeField] private Transform holdPoint;
        [Tooltip("Tilt in degrees while the tool is held (Figma: the held tool leans).")]
        [SerializeField] private float heldTilt = 20f;

        private SpriteRenderer spriteRenderer;
        private Vector3 homePosition;
        private Quaternion homeRotation;
        private int homeOrder;

        /// <summary>False when the tool can't be used now (an empty seed packet).</summary>
        public virtual bool CanPickUp => true;

        /// <summary>True while the player is currently holding and dragging this tool.</summary>
        public bool IsHeld { get; private set; }

        protected virtual void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            homePosition = transform.position;
            homeRotation = transform.rotation;
            homeOrder = spriteRenderer.sortingOrder;
        }

        public void PickUp()
        {
            IsHeld = true;
            SetTilt(heldTilt);
            spriteRenderer.sortingOrder = homeOrder + HeldOrderBoost;
            OnPickedUp();
        }

        /// <summary>Moves the tool so that its hold point (or work point) sits under the cursor.</summary>
        public void MoveTo(Vector2 cursor)
        {
            Transform grip = holdPoint != null ? holdPoint : workPoint;
            Vector2 offset = grip != null ? (Vector2)(grip.position - transform.position) : Vector2.zero;
            Vector2 position = cursor - offset;
            transform.position = new Vector3(position.x, position.y, homePosition.z);
        }

        public void ReturnHome()
        {
            IsHeld = false;
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
