using UnityEngine;
using UnityEngine.UI;

namespace SproutOfDebt.Laboratory.UI
{
    public class StirTimerView : MonoBehaviour
    {
        [SerializeField] private BrewingStation station;
        [SerializeField] private GameObject root;
        [SerializeField] private Image ring;

        private void Awake()
        {
            SetVisible(false);
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

        private void Refresh()
        {
            bool stirring = station.IsBrewing && station.CurrentStep.Type == BrewingStepType.Stir;
            SetVisible(stirring);
            if (stirring && ring != null)
            {
                ring.fillAmount = station.StirProgress;
            }
        }

        private void SetVisible(bool visible)
        {
            if (root != null)
            {
                root.SetActive(visible);
            }
        }
    }
}
