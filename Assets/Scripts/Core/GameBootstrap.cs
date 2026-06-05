using UnityEngine;

namespace Autobazar.Core
{
    /// <summary>
    /// Nejjednodušší způsob, jak hru spustit: do prázdné scény dej jeden prázdný
    /// GameObject s touto komponentou a stiskni Play. Postaví se celý svět z kódu.
    ///
    /// (Alternativa: v menu "Autobazar/Build Phase 1 Scene" postavit svět rovnou
    /// do scény a uložit ji – pak GameBootstrap není potřeba.)
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        [Tooltip("Postavit svět automaticky při spuštění hry.")]
        [SerializeField] private bool buildOnPlay = true;

        private void Awake()
        {
            if (buildOnPlay) WorldBuilder.BuildWorld();
        }
    }
}
