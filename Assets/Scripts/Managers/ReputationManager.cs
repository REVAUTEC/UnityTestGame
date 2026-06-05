using System;
using UnityEngine;

namespace Autobazar.Managers
{
    /// <summary>
    /// Reputace autobazaru v rozsahu 0–100. Singleton.
    /// V pozdějších fázích ovlivní, jak často chodí zákazníci.
    /// </summary>
    public class ReputationManager : MonoBehaviour
    {
        public static ReputationManager Instance { get; private set; }

        [SerializeField] private int startingReputation = 50;

        public int Reputation { get; private set; }

        public event Action<int> OnReputationChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            Reputation = Mathf.Clamp(startingReputation, 0, 100);
        }

        /// <summary>Změní reputaci o danou hodnotu (kladnou i zápornou) a ořízne na 0–100.</summary>
        public void ChangeReputation(int delta)
        {
            Reputation = Mathf.Clamp(Reputation + delta, 0, 100);
            OnReputationChanged?.Invoke(Reputation);
        }
    }
}
