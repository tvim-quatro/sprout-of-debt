using System;
using UnityEngine;

namespace SproutOfDebt.Core.Save
{
    /// <summary>
    /// Player progress through the first-time onboarding tutorial.
    /// </summary>
    [Serializable]
    public class TutorialProgress
    {
        [SerializeField] private TutorialStep currentStep = TutorialStep.IntroMoneylender1;
        [SerializeField] private bool isCompleted;

        public TutorialStep CurrentStep
        {
            get => currentStep;
            set => currentStep = value;
        }

        public bool IsCompleted
        {
            get => isCompleted;
            set => isCompleted = value;
        }
    }
}
