using SproutOfDebt.Core.Save;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SproutOfDebt.Garden.UI
{
    /// <summary>
    /// The status indicator above one garden cell (Figma component "garden_plot_status").
    /// According to GDD: a water drop when the plant needs watering, a filling progress ring while it grows
    /// and a harvest icon when it is ready; empty cells show no indicator.
    /// The ring also fills while the cell is being watered. Only displays the cell's state, owns no game rules.
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    public class PlotStatusView : MonoBehaviour
    {
        // The timer plate (45 px) and the 6 px gap under the badge while the plant grows (Figma).
        private const float TimerHeight = 51f;

        [SerializeField] private GardenPlot plot;
        [Tooltip("The plant sprite of the cell; the indicator stays just above it.")]
        [SerializeField] private SpriteRenderer plantRenderer;

        [Header("Parts")]
        [SerializeField] private RectTransform badge;
        [SerializeField] private Image badgeImage;
        [Tooltip("Progress ring, an Image filled radially from the top, clockwise.")]
        [SerializeField] private Image ring;
        [SerializeField] private GameObject timer;
        [SerializeField] private TMP_Text timerText;

        [Header("Badges")]
        [SerializeField] private Sprite needsWaterBadge;
        [SerializeField] private Sprite growingBadge;
        [SerializeField] private Sprite readyBadge;

        [Header("Layout")]
        [Tooltip("Gap between the indicator and the top of the plant, in world units (Figma: 8 px).")]
        [SerializeField] private float plantGap = 0.08f;
        [Tooltip("Height of the indicator bottom above the cell center while nothing grows yet, in world units (Figma: 85.5 px).")]
        [SerializeField] private float sownHeight = 0.855f;

        private readonly char[] timeChars = new char[8];
        private Canvas canvas;
        private int shownSeconds = -1;

        private void Awake()
        {
            canvas = GetComponent<Canvas>();
            canvas.enabled = false;
        }

        private void OnEnable()
        {
            if (plot != null)
            {
                plot.StateChanged += OnStateChanged;
            }
        }

        private void OnDisable()
        {
            if (plot != null)
            {
                plot.StateChanged -= OnStateChanged;
            }
        }

        // The cell loads its saved state in Awake, so the first refresh waits for Start.
        private void Start()
        {
            Refresh();
        }

        private void LateUpdate()
        {
            if (!canvas.enabled)
            {
                return;
            }
            float fill = plot.State == PlotState.Growing ? plot.Progress : plot.State == PlotState.Sown ? plot.WaterProgress : 0f;
            ring.enabled = fill > 0f;
            ring.fillAmount = fill;
            if (plot.State == PlotState.Growing)
            {
                ShowTime(Mathf.CeilToInt(plot.RemainingSeconds));
            }
            // The plant grows taller with every stage, so the indicator follows its top.
            transform.position = new Vector3(plot.transform.position.x, BottomY(), transform.position.z);
        }

        private void OnStateChanged(GardenPlot changed)
        {
            Refresh();
        }

        private void Refresh()
        {
            if (plot == null)
            {
                return;
            }
            Sprite sprite = BadgeFor(plot.State);
            canvas.enabled = sprite != null;
            if (sprite == null)
            {
                return;
            }
            badgeImage.sprite = sprite;
            bool growing = plot.State == PlotState.Growing;
            timer.SetActive(growing);
            badge.anchoredPosition = new Vector2(0f, growing ? TimerHeight : 0f);
            shownSeconds = -1;
            LateUpdate();
        }

        private Sprite BadgeFor(PlotState state)
        {
            switch (state)
            {
                case PlotState.Sown: return needsWaterBadge;
                case PlotState.Growing: return growingBadge;
                case PlotState.Ripe: return readyBadge;
                default: return null;
            }
        }

        private float BottomY()
        {
            if (plantRenderer != null && plantRenderer.sprite != null)
            {
                return plantRenderer.bounds.max.y + plantGap;
            }
            return plot.transform.position.y + sownHeight;
        }

        /// <summary>Shows the time left as m:ss, rebuilding the text only when the seconds change (no allocations).</summary>
        private void ShowTime(int seconds)
        {
            if (seconds == shownSeconds || timerText == null)
            {
                return;
            }
            shownSeconds = seconds;
            int length = 0;
            int minutes = seconds / 60;
            if (minutes >= 10)
            {
                timeChars[length++] = (char)('0' + minutes / 10 % 10);
            }
            timeChars[length++] = (char)('0' + minutes % 10);
            timeChars[length++] = ':';
            timeChars[length++] = (char)('0' + seconds % 60 / 10);
            timeChars[length++] = (char)('0' + seconds % 10);
            timerText.SetCharArray(timeChars, 0, length);
        }
    }
}
