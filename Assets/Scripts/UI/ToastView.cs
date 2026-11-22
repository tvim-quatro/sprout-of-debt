using System.Collections;
using SproutOfDebt.Core;
using TMPro;
using UnityEngine;

namespace SproutOfDebt.UI
{
    /// <summary>
    /// The toast banner (Figma component "Toast": ui_toast_banner with a short bold line on it).
    /// It fades in, stays for a moment and fades out; a new message restarts it. It never takes clicks.
    /// Lives in the Panel_Toast prefab that scene builders place; gameplay code reaches it through GameServices.Toasts.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class ToastView : MonoBehaviour, IToastService
    {
        [SerializeField] private TMP_Text messageText;

        [Header("Timing")]
        [SerializeField, Min(0f)] private float fadeSeconds = 0.2f;
        [Tooltip("How long the message stays fully visible, in seconds.")]
        [SerializeField, Min(0f)] private float showSeconds = 1.5f;

        private CanvasGroup group;
        private Coroutine playing;

        private void Awake()
        {
            group = GetComponent<CanvasGroup>();
            group.interactable = false;
            group.blocksRaycasts = false;
            group.alpha = 0f;
        }

        private void OnEnable()
        {
            GameServices.Toasts = this;
        }

        private void OnDisable()
        {
            if (ReferenceEquals(GameServices.Toasts, this))
            {
                GameServices.Toasts = null;
            }
            StopAllCoroutines();
            playing = null;
            group.alpha = 0f;
        }

        public void Show(string message)
        {
            if (messageText != null)
            {
                messageText.text = message;
            }
            if (playing != null)
            {
                StopCoroutine(playing);
            }
            playing = StartCoroutine(Play());
        }

        private IEnumerator Play()
        {
            yield return Fade(group.alpha, 1f);
            yield return new WaitForSeconds(showSeconds);
            yield return Fade(1f, 0f);
            playing = null;
        }

        private IEnumerator Fade(float from, float to)
        {
            for (float time = 0f; time < fadeSeconds; time += Time.deltaTime)
            {
                group.alpha = Mathf.Lerp(from, to, time / fadeSeconds);
                yield return null;
            }
            group.alpha = to;
        }
    }
}
