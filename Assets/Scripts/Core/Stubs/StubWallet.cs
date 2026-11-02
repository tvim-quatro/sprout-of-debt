using System;
using UnityEngine;

namespace SproutOfDebt.Core.Stubs
{
    /// <summary>In-memory wallet used until the real one exists.</summary>
    public class StubWallet : IWallet
    {
        public int Coins { get; private set; }

        public event Action<int> CoinsChanged;

        public StubWallet(int startingCoins)
        {
            Coins = Mathf.Max(0, startingCoins);
        }

        public void Add(int amount)
        {
            if (amount <= 0) return;
            Coins += amount;
            CoinsChanged?.Invoke(Coins);
        }

        public bool TrySpend(int amount)
        {
            if (amount < 0 || amount > Coins) return false;
            if (amount == 0) return true;
            Coins -= amount;
            CoinsChanged?.Invoke(Coins);
            return true;
        }
    }
}
