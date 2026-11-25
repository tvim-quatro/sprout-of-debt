using System;

namespace SproutOfDebt.Core
{
    /// <summary>
    /// Service that coordinates the player onboarding / initial gameplay loop tutorial.
    /// </summary>
    public interface ITutorialService
    {
        bool IsActive { get; }
        TutorialStep CurrentStep { get; }
        bool IsCompleted { get; }

        event Action<TutorialStep> StepChanged;
        event Action Completed;

        void Advance();
        void CompleteStep(TutorialStep step);
        void CompleteTutorial();
        void SetStep(TutorialStep step);
    }
}
