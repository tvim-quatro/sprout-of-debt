using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SproutOfDebt.Shop.UI
{
    /// <summary>Debt repayment progress bar with a marker for every recipe milestone.</summary>
    public class DebtBarView : MonoBehaviour
    {
        [SerializeField] private RectTransform fill;
        [SerializeField] private RectTransform markersRoot;
        [SerializeField] private Sprite markerLocked;
        [SerializeField] private Sprite markerReached;
        [SerializeField] private TMP_Text label;
        [Tooltip("{0} = coins repaid, {1} = total debt")]
        [SerializeField] private string labelFormat = "DEBT REPAID  {0} / {1}";
        [Tooltip("Show the potion icon each milestone unlocks under its marker")]
        [SerializeField] private bool showPotionIcons;
        [SerializeField] private Vector2 potionIconSize = new Vector2(39, 55);

        private readonly List<(DebtMilestone milestone, Image marker)> markers = new List<(DebtMilestone, Image)>();
        private Treasury treasury;

        private void Start()
        {
            treasury = ShopContext.Treasury;
            if (treasury == null) return;
            BuildMarkers();
            treasury.Changed += Refresh;
            Refresh();
        }

        private void OnEnable()
        {
            if (treasury != null) Refresh();
        }

        private void OnDestroy()
        {
            if (treasury != null) treasury.Changed -= Refresh;
        }

        private void BuildMarkers()
        {
            foreach (var milestone in treasury.Config.milestones)
            {
                if (milestone.threshold <= 0 || milestone.threshold >= treasury.TotalDebt) continue;
                float x = (float)milestone.threshold / treasury.TotalDebt;

                var marker = CreateImage("Marker", markersRoot, markerLocked);
                marker.SetNativeSize();
                var rt = marker.rectTransform;
                rt.anchorMin = rt.anchorMax = new Vector2(x, 0.5f);
                rt.anchoredPosition = Vector2.zero;
                markers.Add((milestone, marker));

                if (showPotionIcons && milestone.potion != null)
                {
                    var icon = CreateImage("PotionIcon", markersRoot, milestone.potion.Icon);
                    var iconRt = icon.rectTransform;
                    iconRt.anchorMin = iconRt.anchorMax = new Vector2(x, 0f);
                    iconRt.pivot = new Vector2(0.5f, 1f);
                    iconRt.sizeDelta = potionIconSize;
                    iconRt.anchoredPosition = new Vector2(0, -12);
                    icon.preserveAspect = true;
                }
            }
        }

        private void Refresh()
        {
            fill.anchorMax = new Vector2(treasury.Progress, fill.anchorMax.y);
            fill.gameObject.SetActive(treasury.Repaid > 0);
            foreach (var (milestone, marker) in markers)
            {
                marker.sprite = treasury.Repaid >= milestone.threshold ? markerReached : markerLocked;
            }
            if (label != null) label.text = string.Format(labelFormat, treasury.Repaid, treasury.TotalDebt);
        }

        private static Image CreateImage(string name, Transform parent, Sprite sprite)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.raycastTarget = false;
            return image;
        }
    }
}
