using System;

namespace DrugCrafter.Core
{
    /// <summary>Player's coin balance. Coins are the only currency in the game.</summary>
    public interface IWallet
    {
        int Coins { get; }

        /// <summary>Raised with the new balance whenever it changes.</summary>
        event Action<int> CoinsChanged;

        void Add(int amount);

        /// <summary>Spends coins if the balance is high enough. Returns false and spends nothing otherwise.</summary>
        bool TrySpend(int amount);
    }
}
