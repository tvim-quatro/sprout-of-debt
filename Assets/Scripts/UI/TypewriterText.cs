using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace SproutOfDebt.UI
{
    /// <summary>
    /// Types a character's line out letter by letter, like the customer dialogue at the Counter.
    /// The full text is set at once, so the layout keeps its final size while the letters appear.
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public class TypewriterText : MonoBehaviour
    {
        [SerializeField, Min(1f)] private float charactersPerSecond = 45f;

        private TMP_Text text;
        private Coroutine routine;

        public bool IsTyping => routine != null;

        public event Action Finished;

        private void Awake()
        {
            text = GetComponent<TMP_Text>();
        }

        private void OnDisable()
        {
            // A hidden line is never left half-typed.
            if (routine != null)
            {
                routine = null;
                text.maxVisibleCharacters = int.MaxValue;
            }
        }

        public void Play(string value)
        {
            if (text == null) text = GetComponent<TMP_Text>();
            Stop();
            text.text = value;
            if (!isActiveAndEnabled || string.IsNullOrEmpty(value))
            {
                Complete();
                return;
            }
            text.maxVisibleCharacters = 0;
            routine = StartCoroutine(Type());
        }

        /// <summary>Shows the whole line at once (the player clicked while it was typing).</summary>
        public void Complete()
        {
            Stop();
            text.maxVisibleCharacters = int.MaxValue;
            Finished?.Invoke();
        }

        private void Stop()
        {
            if (routine == null) return;
            StopCoroutine(routine);
            routine = null;
        }

        private IEnumerator Type()
        {
            text.ForceMeshUpdate();
            int total = text.textInfo.characterCount;
            // Unscaled, so the line still types while a window pauses the game.
            for (float shown = 0f; shown < total; shown += charactersPerSecond * Time.unscaledDeltaTime)
            {
                text.maxVisibleCharacters = (int)shown;
                yield return null;
            }
            routine = null;
            text.maxVisibleCharacters = int.MaxValue;
            Finished?.Invoke();
        }
    }
}
