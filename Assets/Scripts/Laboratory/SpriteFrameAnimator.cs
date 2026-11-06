using UnityEngine;

namespace SproutOfDebt.Laboratory
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteFrameAnimator : MonoBehaviour
    {
        [SerializeField] private Sprite[] frames;
        [SerializeField, Min(0.1f)] private float framesPerSecond = 6f;

        private SpriteRenderer spriteRenderer;
        private float elapsed;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void OnEnable()
        {
            elapsed = 0f;
            ShowFrame(0);
        }

        private void Update()
        {
            if (frames == null || frames.Length < 2)
            {
                return;
            }

            elapsed += Time.deltaTime;
            ShowFrame((int)(elapsed * framesPerSecond) % frames.Length);
        }

        private void ShowFrame(int index)
        {
            if (spriteRenderer != null && frames != null && frames.Length > 0)
            {
                spriteRenderer.sprite = frames[index];
            }
        }
    }
}
