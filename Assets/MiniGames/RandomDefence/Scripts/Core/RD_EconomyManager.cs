using System;
using UnityEngine;

namespace MiniGames.RandomDefence
{
    public class RD_EconomyManager : MonoBehaviour
    {
        [Header("초기값")]
        [SerializeField] private int startCurrency = 100;
        [SerializeField] private int maxLives      = 40;

        public int         currency { get; private set; }
        public Action<int> onCurrencyChanged;

        public int         starCurrency { get; private set; }
        public Action<int> onStarChanged;

        public int         lives { get; private set; }
        public Action<int> onLivesChanged;

        public void Initialize()
        {
            currency     = startCurrency;
            starCurrency = 0;
            lives        = maxLives;
            onCurrencyChanged?.Invoke(currency);
            onStarChanged?.Invoke(starCurrency);
            onLivesChanged?.Invoke(lives);
        }

        public void AddCurrency(int amount)
        {
            currency += amount;
            onCurrencyChanged?.Invoke(currency);
        }

        public bool TrySpend(int amount)
        {
            if (currency < amount) return false;
            currency -= amount;
            onCurrencyChanged?.Invoke(currency);
            return true;
        }

        public bool CanAfford(int amount) => currency >= amount;

        public void AddStar(int amount)
        {
            starCurrency += amount;
            onStarChanged?.Invoke(starCurrency);
        }

        public bool TrySpendStar(int amount)
        {
            if (starCurrency < amount) return false;
            starCurrency -= amount;
            onStarChanged?.Invoke(starCurrency);
            return true;
        }

        public bool LoseLife(int amount = 1)
        {
            lives = Mathf.Max(lives - amount, 0);
            onLivesChanged?.Invoke(lives);
            return lives > 0;
        }
    }
}
