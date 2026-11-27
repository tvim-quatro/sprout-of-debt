using System;
using SproutOfDebt.Core;
using SproutOfDebt.UI;
using UnityEngine;

namespace SproutOfDebt.Tutorial.UI
{
    /// <summary>
    /// Coordinates the tutorial UI on the shared HUD Canvas:
    /// Moneylender dialogue, Imp hint card, dim overlay with cutout, and location lock enforcement.
    /// </summary>
    public class TutorialOverlayController : MonoBehaviour
    {
        [Header("Views")]
        [SerializeField] private MoneylenderDialogueView moneylenderView;
        [SerializeField] private TutorialHintView hintView;
        [SerializeField] private TutorialDimMask dimMask;

        [Header("Location Nav")]
        [SerializeField] private LocationNavBar navBar;

        private ITutorialService subscribedService;
        private RectTransform canvasRect;
        private Camera mainCamera;
        private TutorialStep lastRenderedStep = (TutorialStep)(-1);

        private void Awake()
        {
            canvasRect = transform.parent as RectTransform ?? transform.root.GetComponentInChildren<Canvas>()?.GetComponent<RectTransform>();
            mainCamera = Camera.main;

            if (moneylenderView != null)
            {
                moneylenderView.NextClicked += OnNextClicked;
            }
            if (hintView != null)
            {
                hintView.NextClicked += OnNextClicked;
            }
        }

        private void OnEnable()
        {
            EnsureSubscribed();
            RefreshStep(true);
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        // The tutorial service may be registered after this view is enabled, so keep checking until we are subscribed to it.
        private void EnsureSubscribed()
        {
            ITutorialService service = GameServices.Tutorial;
            if (ReferenceEquals(service, subscribedService))
            {
                return;
            }

            Unsubscribe();
            if (service != null)
            {
                service.StepChanged += OnStepChanged;
                service.Completed += OnTutorialCompleted;
                subscribedService = service;
                RefreshStep(true);
            }
        }

        private void Unsubscribe()
        {
            if (subscribedService != null)
            {
                subscribedService.StepChanged -= OnStepChanged;
                subscribedService.Completed -= OnTutorialCompleted;
                subscribedService = null;
            }
        }

        private void LateUpdate()
        {
            EnsureSubscribed();

            if (GameServices.Tutorial == null || GameServices.Tutorial.IsCompleted)
            {
                return;
            }

            // Continuously update cutout in case targets move or are animated/opened
            TutorialStep current = GameServices.Tutorial.CurrentStep;
            if (TutorialStepCatalog.TryGet(current, out TutorialStepData data) && data.highlight != HighlightTarget.None)
            {
                if (TutorialTargetLocator.TryGetTargetRect(data.highlight, canvasRect, mainCamera, out Rect hole))
                {
                    dimMask.SetCutout(hole);
                }
            }

            EnforceLocationLocks(current);
        }

        private void OnStepChanged(TutorialStep step)
        {
            RefreshStep(false);
        }

        private void OnTutorialCompleted()
        {
            HideAll();
            UnlockAllLocations();
        }

        private void OnNextClicked()
        {
            if (GameServices.Tutorial == null) return;

            TutorialStep current = GameServices.Tutorial.CurrentStep;

            // When Moneylender gives seeds, grant 3 lavender seeds
            if (current == TutorialStep.IntroMoneylender3)
            {
                GrantInitialSeeds();
            }

            GameServices.Tutorial.Advance();
        }

        private void RefreshStep(bool force)
        {
            if (GameServices.Tutorial == null || GameServices.Tutorial.IsCompleted)
            {
                HideAll();
                UnlockAllLocations();
                return;
            }

            TutorialStep current = GameServices.Tutorial.CurrentStep;
            if (!force && current == lastRenderedStep)
            {
                return;
            }
            lastRenderedStep = current;

            if (!TutorialStepCatalog.TryGet(current, out TutorialStepData data))
            {
                HideAll();
                return;
            }

            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            bool isCounter = string.Equals(currentScene, "ShopCounter", StringComparison.OrdinalIgnoreCase);

            if (data.character == CharacterType.Moneylender)
            {
                hintView.Hide();
                moneylenderView.Show(data.thought, data.mood);
            }
            else if (data.character == CharacterType.Imp)
            {
                moneylenderView.Hide();
                if (hintView != null)
                {
                    hintView.SetBottomPosition(isCounter);
                }
                hintView.Show(data.thought, data.action, data.showAction, data.showNext, data.mood);
            }
            else
            {
                moneylenderView.Hide();
                hintView.Hide();
            }

            if (data.highlight == HighlightTarget.None)
            {
                dimMask.ShowFullscreenDim();
            }
            else if (TutorialTargetLocator.TryGetTargetRect(data.highlight, canvasRect, mainCamera, out Rect hole))
            {
                dimMask.SetCutout(hole);
            }
            else
            {
                dimMask.ShowFullscreenDim();
            }

            EnforceLocationLocks(current);
        }

        private void EnforceLocationLocks(TutorialStep step)
        {
            if (navBar == null)
            {
                navBar = FindAnyObjectByType<LocationNavBar>();
            }
            if (navBar == null) return;

            bool labUnlocked = step >= TutorialStep.GardenGoToLab;
            bool counterUnlocked = step >= TutorialStep.LabGoToCounter;

            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            bool isGarden = string.Equals(currentScene, "Garden", StringComparison.OrdinalIgnoreCase);
            bool isLab = string.Equals(currentScene, "Laboratory", StringComparison.OrdinalIgnoreCase);
            bool isCounter = string.Equals(currentScene, "ShopCounter", StringComparison.OrdinalIgnoreCase);

            if (navBar.GardenButton != null)
            {
                navBar.GardenButton.interactable = !isGarden;
            }
            if (navBar.LaboratoryButton != null)
            {
                navBar.LaboratoryButton.interactable = labUnlocked && !isLab;
            }
            if (navBar.CounterButton != null)
            {
                navBar.CounterButton.interactable = counterUnlocked && !isCounter;
            }
        }

        private void UnlockAllLocations()
        {
            if (navBar == null)
            {
                navBar = FindAnyObjectByType<LocationNavBar>();
            }
            if (navBar != null)
            {
                string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                bool isGarden = string.Equals(currentScene, "Garden", StringComparison.OrdinalIgnoreCase);
                bool isLab = string.Equals(currentScene, "Laboratory", StringComparison.OrdinalIgnoreCase);
                bool isCounter = string.Equals(currentScene, "ShopCounter", StringComparison.OrdinalIgnoreCase);

                if (navBar.GardenButton != null) navBar.GardenButton.interactable = !isGarden;
                if (navBar.LaboratoryButton != null) navBar.LaboratoryButton.interactable = !isLab;
                if (navBar.CounterButton != null) navBar.CounterButton.interactable = !isCounter;
            }
        }

        private void GrantInitialSeeds()
        {
            if (GameServices.Inventory != null)
            {
                if (GameServices.Inventory.GetCount("seed_lavender") == 0)
                {
                    GameServices.Inventory.Add("seed_lavender", 3);
                }
            }
        }

        private void HideAll()
        {
            if (moneylenderView != null) moneylenderView.Hide();
            if (hintView != null) hintView.Hide();
            if (dimMask != null) dimMask.Hide();
        }
    }
}
