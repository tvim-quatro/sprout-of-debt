using System;
using SproutOfDebt.Core;
using SproutOfDebt.Tutorial.Hooks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SproutOfDebt.Tutorial
{
    /// <summary>
    /// Master coordinator for onboarding tutorial.
    /// Manages current step, persistence in GameProgress, and location transition permissions.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class TutorialManager : MonoBehaviour, ITutorialService
    {
        public static TutorialManager Instance { get; private set; }

        public bool IsActive => !IsCompleted;
        public TutorialStep CurrentStep => GameServices.Progress.Tutorial.CurrentStep;
        public bool IsCompleted => GameServices.Progress.Tutorial.IsCompleted;

        public event Action<TutorialStep> StepChanged;
        public event Action Completed;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            GameServices.Tutorial = this;

            SceneManager.sceneLoaded += OnSceneLoaded;
            EnsureSceneHook(SceneManager.GetActiveScene().name);
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;

            if (ReferenceEquals(GameServices.Tutorial, this))
            {
                GameServices.Tutorial = null;
            }
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            EnsureSceneHook(scene.name);
        }

        private void EnsureSceneHook(string sceneName)
        {
            if (IsCompleted) return;

            if (sceneName == "Garden" && FindFirstObjectByType<GardenTutorialHook>() == null)
            {
                var go = new GameObject("[TutorialHook_Garden]");
                go.transform.SetParent(transform, false);
                go.AddComponent<GardenTutorialHook>();
            }
            else if (sceneName == "Laboratory" && FindFirstObjectByType<LaboratoryTutorialHook>() == null)
            {
                var go = new GameObject("[TutorialHook_Laboratory]");
                go.transform.SetParent(transform, false);
                go.AddComponent<LaboratoryTutorialHook>();
            }
            else if (sceneName == "ShopCounter" && FindFirstObjectByType<ShopTutorialHook>() == null)
            {
                var go = new GameObject("[TutorialHook_Shop]");
                go.transform.SetParent(transform, false);
                go.AddComponent<ShopTutorialHook>();
            }
        }

        public void Advance()
        {
            if (IsCompleted) return;

            TutorialStep next = GetNextStep(CurrentStep);
            SetStep(next);
        }

        public void CompleteStep(TutorialStep step)
        {
            if (IsCompleted) return;
            if (CurrentStep == step)
            {
                Advance();
            }
        }

        public void CompleteTutorial()
        {
            GameServices.Progress.Tutorial.IsCompleted = true;
            GameServices.Progress.Tutorial.CurrentStep = TutorialStep.Completed;
            StepChanged?.Invoke(TutorialStep.Completed);
            Completed?.Invoke();
        }

        public void SetStep(TutorialStep step)
        {
            if (step == TutorialStep.Completed)
            {
                CompleteTutorial();
                return;
            }

            GameServices.Progress.Tutorial.CurrentStep = step;
            StepChanged?.Invoke(step);
        }

        private static TutorialStep GetNextStep(TutorialStep current)
        {
            return current switch
            {
                TutorialStep.IntroMoneylender1 => TutorialStep.IntroMoneylender2,
                TutorialStep.IntroMoneylender2 => TutorialStep.IntroMoneylender3,
                TutorialStep.IntroMoneylender3 => TutorialStep.GardenRake,

                TutorialStep.GardenRake => TutorialStep.GardenTill,
                TutorialStep.GardenTill => TutorialStep.GardenOpenInventory,
                TutorialStep.GardenOpenInventory => TutorialStep.GardenPickSeeds,
                TutorialStep.GardenPickSeeds => TutorialStep.GardenSow,
                TutorialStep.GardenSow => TutorialStep.GardenWateringCan,
                TutorialStep.GardenWateringCan => TutorialStep.GardenWater,
                TutorialStep.GardenWater => TutorialStep.GardenGrowthTimer,
                TutorialStep.GardenGrowthTimer => TutorialStep.GardenHarvestFirst,
                TutorialStep.GardenHarvestFirst => TutorialStep.GardenHarvestRest,
                TutorialStep.GardenHarvestRest => TutorialStep.GardenGoToLab,
                TutorialStep.GardenGoToLab => TutorialStep.LabOpenRecipe,

                TutorialStep.LabOpenRecipe => TutorialStep.LabClickBrew,
                TutorialStep.LabClickBrew => TutorialStep.LabLightFire,
                TutorialStep.LabLightFire => TutorialStep.LabOpenInventory,
                TutorialStep.LabOpenInventory => TutorialStep.LabPickIngredient,
                TutorialStep.LabPickIngredient => TutorialStep.LabAddIngredients,
                TutorialStep.LabAddIngredients => TutorialStep.LabStir,
                TutorialStep.LabStir => TutorialStep.LabPotionReady,
                TutorialStep.LabPotionReady => TutorialStep.LabGoToCounter,
                TutorialStep.LabGoToCounter => TutorialStep.CounterCustomerRequest,

                TutorialStep.CounterCustomerRequest => TutorialStep.CounterSellPotion,
                TutorialStep.CounterSellPotion => TutorialStep.CounterPotionSold,
                TutorialStep.CounterPotionSold => TutorialStep.CounterClickDebtBar,
                TutorialStep.CounterClickDebtBar => TutorialStep.CounterPayDebt,
                TutorialStep.CounterPayDebt => TutorialStep.CounterMoneylenderFarewell,
                TutorialStep.CounterMoneylenderFarewell => TutorialStep.Completed,

                _ => TutorialStep.Completed
            };
        }
    }
}
