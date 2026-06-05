using System;
using UnityEngine;

namespace Autobazar.Managers
{
    /// <summary>
    /// Spravuje peníze a počet prodaných aut. Singleton – přístup přes EconomyManager.Instance.
    /// </summary>
    public class EconomyManager : MonoBehaviour
    {
        public static EconomyManager Instance { get; private set; }

        [SerializeField] private int startingMoney = 10000;

        public int Money { get; private set; }
        public int CarsSold { get; private set; }

        // Eventy, na které se napojí UI (push aktualizace).
        public event Action<int> OnMoneyChanged;
        public event Action<int> OnCarsSoldChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            Money = startingMoney;
        }

        /// <summary>Přidá peníze (např. po prodeji auta).</summary>
        public void AddMoney(int amount)
        {
            Money += amount;
            OnMoneyChanged?.Invoke(Money);
        }

        /// <summary>Zkusí utratit peníze. Vrátí false, pokud není dost.</summary>
        public bool TrySpendMoney(int amount)
        {
            if (amount > Money) return false;
            Money -= amount;
            OnMoneyChanged?.Invoke(Money);
            return true;
        }

        /// <summary>Zaeviduje prodej auta.</summary>
        public void RegisterCarSold()
        {
            CarsSold++;
            OnCarsSoldChanged?.Invoke(CarsSold);
        }
    }
}
