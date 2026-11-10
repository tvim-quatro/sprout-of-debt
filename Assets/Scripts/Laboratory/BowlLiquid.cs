using UnityEngine;

namespace SproutOfDebt.Laboratory
{
    public class BowlLiquid : MonoBehaviour
    {
        [SerializeField] private BrewingStation station;
        [SerializeField] private SpriteRenderer liquid;
        [SerializeField, Range(0f, 1f)] private float maxAlpha = 0.7f;
        [SerializeField, Min(0.1f)] private float fadeSpeed = 1.2f;

        private float alpha;
        private float targetAlpha;

        private void Awake()
        {
            Apply();
        }

        private void OnEnable()
        {
            if (station == null)
            {
                return;
            }
            station.StepChanged += Refresh;
            station.Stirred += Refresh;
        }

        private void OnDisable()
        {
            if (station == null)
            {
                return;
            }
            station.StepChanged -= Refresh;
            station.Stirred -= Refresh;
        }

        private void Update()
        {
            if (Mathf.Approximately(alpha, targetAlpha))
            {
                return;
            }
            alpha = Mathf.MoveTowards(alpha, targetAlpha, fadeSpeed * Time.deltaTime);
            Apply();
        }

        private void Refresh()
        {
            if (station.IsBrewing && liquid != null)
            {
                Color color = station.CurrentRecipe.LiquidColor;
                color.a = liquid.color.a;
                liquid.color = color;
            }
            targetAlpha = station.IsBrewing ? maxAlpha * station.TotalStirProgress : 0f;
        }

        private void Apply()
        {
            if (liquid == null)
            {
                return;
            }
            Color color = liquid.color;
            color.a = alpha;
            liquid.color = color;
        }
    }
}
