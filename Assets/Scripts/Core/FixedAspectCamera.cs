using UnityEngine;

namespace SproutOfDebt.Core
{
    /// <summary>
    /// Keeps the whole 1920x1080 world frame visible on any screen. The orthographic size grows on screens
    /// narrower than 16:9; on wider screens the extra width shows the black clear color, matching the UI frame.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class FixedAspectCamera : MonoBehaviour
    {
        [Tooltip("Visible world area in units (Figma 1920x1080 at 100 Pixels Per Unit).")]
        [SerializeField] private Vector2 frameSize = new Vector2(19.2f, 10.8f);

        private Camera targetCamera;
        private float fittedAspect;

        private void Awake()
        {
            targetCamera = GetComponent<Camera>();
        }

        // There is no event for a window resize, so compare one float per frame.
        private void LateUpdate()
        {
            if (Mathf.Approximately(targetCamera.aspect, fittedAspect))
            {
                return;
            }
            fittedAspect = targetCamera.aspect;
            float heightForWidth = frameSize.x / fittedAspect;
            targetCamera.orthographicSize = Mathf.Max(frameSize.y, heightForWidth) / 2f;
        }
    }
}
