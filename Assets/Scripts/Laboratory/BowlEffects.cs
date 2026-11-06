using UnityEngine;

namespace SproutOfDebt.Laboratory
{
    public class BowlEffects : MonoBehaviour
    {
        [SerializeField] private BrewingStation station;
        [SerializeField] private ParticleSystem splash;
        [SerializeField] private ParticleSystem bubbles;
        [SerializeField] private ParticleSystem steam;
        [SerializeField] private ParticleSystem sparkles;
        [SerializeField, Min(0f)] private float bubbleLingerSeconds = 0.3f;

        private float bubblesUntil;

        private void OnEnable()
        {
            if (station == null)
            {
                return;
            }
            station.IngredientAdded += OnIngredientAdded;
            station.Stirred += OnStirred;
            station.PotionBrewed += OnPotionBrewed;
        }

        private void OnDisable()
        {
            if (station == null)
            {
                return;
            }
            station.IngredientAdded -= OnIngredientAdded;
            station.Stirred -= OnStirred;
            station.PotionBrewed -= OnPotionBrewed;
        }

        private void Update()
        {
            if (bubbles != null && bubbles.isEmitting && Time.time > bubblesUntil)
            {
                bubbles.Stop();
            }
        }

        private void OnIngredientAdded()
        {
            Play(splash);
        }

        private void OnStirred()
        {
            bubblesUntil = Time.time + bubbleLingerSeconds;
            if (bubbles != null && !bubbles.isEmitting)
            {
                bubbles.Play();
            }
        }

        private void OnPotionBrewed()
        {
            Play(steam);
            Play(sparkles);
        }

        private static void Play(ParticleSystem system)
        {
            if (system != null)
            {
                system.Play();
            }
        }
    }
}
