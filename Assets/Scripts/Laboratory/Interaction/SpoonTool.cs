using UnityEngine;

namespace SproutOfDebt.Laboratory
{
    public class SpoonTool : LaboratoryTool
    {
        [Header("Held pose")]
        [Tooltip("Z rotation while held. 0 is the sprite's own pose: handle up, head down.")]
        [SerializeField] private float heldAngle;

        [Header("Use point")]
        [Tooltip("How far above the bottom edge of the sprite the stirring point sits, in local units.")]
        [SerializeField, Min(0f)] private float tipInset = 0.25f;

        [Header("Stirring")]
        [Tooltip("Slowest spoon speed that still counts as stirring, in world units per second.")]
        [SerializeField, Min(0f)] private float minStirSpeed = 0.5f;
        [Tooltip("How long stirring keeps counting after the spoon stops, so small pauses in a stroke do not break it.")]
        [SerializeField, Min(0f)] private float stopGraceSeconds = 0.1f;

        [Header("Sway")]
        [SerializeField, Min(0f)] private float maxSwayAngle = 14f;
        [SerializeField, Min(0f)] private float swayPerSpeed = 2f;
        [SerializeField, Min(0.01f)] private float swaySmoothTime = 0.12f;

        private float swayAngle;
        private float swayVelocity;
        private float secondsSinceMoved = float.MaxValue;

        public override bool UseOn(Collider2D target, float deltaTime)
        {
            if (target.TryGetComponent(out Bowl _) && secondsSinceMoved <= stopGraceSeconds)
            {
                Station.Stir(deltaTime);
            }
            return false;
        }

        public override Vector2 GetUsePoint()
        {
            Bounds bounds = SpriteRenderer.sprite.bounds;
            return transform.TransformPoint(new Vector3(bounds.center.x, bounds.min.y + tipInset, 0f));
        }

        protected override void OnPickedUp()
        {
            swayAngle = 0f;
            swayVelocity = 0f;
            secondsSinceMoved = float.MaxValue;
            ApplyRotation();
        }

        protected override void OnMoved(Vector2 delta, float deltaTime)
        {
            float speed = deltaTime > 0f ? delta.magnitude / deltaTime : 0f;
            secondsSinceMoved = speed >= minStirSpeed ? 0f : secondsSinceMoved + deltaTime;

            float speedX = deltaTime > 0f ? delta.x / deltaTime : 0f;
            float target = Mathf.Clamp(speedX * swayPerSpeed, -maxSwayAngle, maxSwayAngle);
            swayAngle = Mathf.SmoothDamp(swayAngle, target, ref swayVelocity, swaySmoothTime);
            ApplyRotation();
        }

        protected override void OnReturnedHome()
        {
            swayAngle = 0f;
            swayVelocity = 0f;
            secondsSinceMoved = float.MaxValue;
        }

        private void ApplyRotation()
        {
            transform.rotation = Quaternion.Euler(0f, 0f, heldAngle + swayAngle);
        }
    }
}
