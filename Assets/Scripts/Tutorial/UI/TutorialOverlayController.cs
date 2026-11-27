using System;
using System.Collections.Generic;
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

        private RectTransform canvasRect;
        private Camera mainCamera;
        private TutorialStep lastRenderedStep = (TutorialStep)(-1);
        private readonly List<Rect> holes = new List<Rect>();
        private readonly List<Rect> frames = new List<Rect>();

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
            if (GameServices.Tutorial != null)
            {
                GameServices.Tutorial.StepChanged += OnStepChanged;
                GameServices.Tutorial.Completed += OnTutorialCompleted;
            }
            RefreshStep(true);
        }

        private void OnDisable()
        {
            if (GameServices.Tutorial != null)
            {
                GameServices.Tutorial.StepChanged -= OnStepChanged;
                GameServices.Tutorial.Completed -= OnTutorialCompleted;
            }
        }

        private void LateUpdate()
        {
            if (GameServices.Tutorial == null || GameServices.Tutorial.IsCompleted)
            {
                return;
            }

            // Continuously update cutout in case targets move or are animated/opened
            TutorialStep current = GameServices.Tutorial.CurrentStep;
            if (TutorialStepCatalog.TryGet(current, out TutorialStepData data) && data.highlight != HighlightTarget.None)
            {
                if (CollectHoles(data))
                {
                    dimMask.SetCutouts(holes, frames, IsActionStep(data));
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
                    hintView.SetTopLeftPosition(data.hintTopLeft);
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
            else if (CollectHoles(data))
            {
                dimMask.SetCutouts(holes, frames, IsActionStep(data));
            }
            else
            {
                dimMask.ShowFullscreenDim();
            }

            EnforceLocationLocks(current);
        }

        // Steps with a Next button only point at their target: it is framed but can't be clicked,
        // so the player can't open something (e.g. the Treasury) that the step can't close again.
        private static bool IsActionStep(TutorialStepData data)
        {
            return !data.showNext;
        }

        // Frames go around the step's highlights (one or two, as in Figma). The dim opens over the same rects,
        // or over the step's cutout area when it has one.
        private bool CollectHoles(TutorialStepData data)
        {
            frames.Clear();
            holes.Clear();
            AddTargetRect(data.highlight, frames);
            AddTargetRect(data.secondaryHighlight, frames);
            if (data.cutoutArea != HighlightTarget.None)
            {
                AddTargetRect(data.cutoutArea, holes);
            }
            if (holes.Count == 0)
            {
                holes.AddRange(frames);
            }
            return holes.Count > 0;
        }

        private void AddTargetRect(HighlightTarget target, List<Rect> rects)
        {
            if (target != HighlightTarget.None &&
                TutorialTargetLocator.TryGetTargetRect(target, canvasRect, mainCamera, out Rect rect))
            {
                rects.Add(rect);
            }
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

            navBar.SetLocked(navBar.GardenButton, false);
            navBar.SetLocked(navBar.LaboratoryButton, !labUnlocked);
            navBar.SetLocked(navBar.CounterButton, !counterUnlocked);
        }

        private void UnlockAllLocations()
        {
            if (navBar == null)
            {
                navBar = FindAnyObjectByType<LocationNavBar>();
            }
            if (navBar == null) return;

            navBar.SetLocked(navBar.GardenButton, false);
            navBar.SetLocked(navBar.LaboratoryButton, false);
            navBar.SetLocked(navBar.CounterButton, false);
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
