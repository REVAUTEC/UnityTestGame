using System;
using UnityEngine;

namespace Autobazar.Managers
{
    /// <summary>
    /// Jednoduchý systém úkolů. Zatím drží jeden aktuální úkol, který se ukazuje v UI.
    /// V dalších fázích se rozšíří na frontu úkolů celé herní smyčky.
    /// </summary>
    public class TaskManager : MonoBehaviour
    {
        public static TaskManager Instance { get; private set; }

        public string CurrentTask { get; private set; } = "";

        public event Action<string> OnTaskChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            // Výchozí úkol.
            SetTask("Počkej na zákazníka u vstupu. Až přijde, dojdi k němu a stiskni E.");
        }

        /// <summary>Nastaví aktuální úkol.</summary>
        public void SetTask(string task)
        {
            CurrentTask = task;
            OnTaskChanged?.Invoke(CurrentTask);
        }
    }
}
