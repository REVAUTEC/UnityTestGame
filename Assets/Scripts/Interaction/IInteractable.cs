using UnityEngine;
using Autobazar.Player;

namespace Autobazar.Interaction
{
    /// <summary>
    /// Cokoliv, s čím může hráč interagovat klávesou E (auto, později počítač v kanceláři,
    /// stojan v servisu, zákazník...). Stačí implementovat tento interface.
    /// </summary>
    public interface IInteractable
    {
        /// <summary>Text výzvy, který se ukáže hráči (např. "Prohlédnout auto").</summary>
        string GetInteractionPrompt();

        /// <summary>Pozice objektu ve světě – kvůli zjištění vzdálenosti od hráče.</summary>
        Transform GetTransform();

        /// <summary>Lze s objektem právě teď interagovat?</summary>
        bool CanInteract();

        /// <summary>Provede interakci.</summary>
        void Interact(PlayerController player);
    }
}
