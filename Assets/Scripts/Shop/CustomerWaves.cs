using UnityEngine;

namespace DrugCrafter.Shop
{
    /// <summary>
    /// Customers come in waves: a random number of customers in a row, then a break of several real minutes
    /// with nobody at the door. Every wave has a different size than the previous one.
    /// The schedule runs on real time while the game is open, also while the player is away from the counter.
    /// </summary>
    public class CustomerWaves
    {
        // ===== Tweak the customer flow here =====
        /// <summary>Most customers in one wave.</summary>
        public const int MaxCustomersPerWave = 10;
        /// <summary>Fewest customers in one wave.</summary>
        public const int MinCustomersPerWave = 3;
        /// <summary>Break between waves, in real minutes (a random value in this range).</summary>
        public const float MinBreakMinutes = 6f;
        public const float MaxBreakMinutes = 10f;
        // ========================================

        private static CustomerWaves current;

        /// <summary>The shared schedule. Survives scene and location changes.</summary>
        public static CustomerWaves Current => current ?? (current = new CustomerWaves());

        private double breakEndsAt;

        public int WaveSize { get; private set; }
        public int CustomersLeft { get; private set; }
        public bool IsOnBreak => CustomersLeft <= 0;
        public float BreakSecondsLeft => IsOnBreak ? Mathf.Max(0f, (float)(breakEndsAt - Now)) : 0f;

        private static double Now => Time.realtimeSinceStartupAsDouble;

        private CustomerWaves()
        {
            StartWave();
        }

        /// <summary>True if a customer may come now. Starts the next wave once the break is over.</summary>
        public bool CanServe()
        {
            if (IsOnBreak && Now >= breakEndsAt) StartWave();
            return !IsOnBreak;
        }

        /// <summary>Call after a customer got an answer (sold or refused).</summary>
        public void OnCustomerServed()
        {
            if (IsOnBreak) return;
            CustomersLeft--;
            if (CustomersLeft <= 0)
            {
                breakEndsAt = Now + Random.Range(MinBreakMinutes, MaxBreakMinutes) * 60f;
            }
        }

        /// <summary>Ends the current break right away (for testing).</summary>
        public void SkipBreak()
        {
            breakEndsAt = Now;
        }

        private void StartWave()
        {
            int size;
            do
            {
                size = Random.Range(MinCustomersPerWave, MaxCustomersPerWave + 1);
            } while (size == WaveSize && MaxCustomersPerWave > MinCustomersPerWave);

            WaveSize = size;
            CustomersLeft = size;
        }
    }
}
