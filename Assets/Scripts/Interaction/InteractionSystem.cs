using System.Collections.Generic;
using UnityEngine;
using Autobazar.Managers;
using Autobazar.Player;

namespace Autobazar.Interaction
{
    /// <summary>
    /// Sedí na hráči. Každý snímek najde nejbližší interaktovatelný objekt
    /// v dosahu a zhruba před hráčem, ukáže výzvu v UI a po stisku E spustí interakci.
    ///
    /// Objekty se registrují samy (CarInteractable v OnEnable), takže se nemusí nic
    /// ručně propojovat. Registr používá MonoBehaviour, aby správně fungovala
    /// Unity kontrola na zničený objekt (== null).
    /// </summary>
    public class InteractionSystem : MonoBehaviour
    {
        [SerializeField] private float interactRange = 3.5f;
        [SerializeField] private float facingDotThreshold = 0.1f; // jak moc musí být objekt před hráčem

        private static readonly List<MonoBehaviour> Registry = new List<MonoBehaviour>();
        private PlayerController _player;
        private IInteractable _current;

        public static void Register(MonoBehaviour interactable)
        {
            if (interactable is IInteractable && !Registry.Contains(interactable))
                Registry.Add(interactable);
        }

        public static void Unregister(MonoBehaviour interactable)
        {
            Registry.Remove(interactable);
        }

        private void Awake()
        {
            _player = GetComponent<PlayerController>();
        }

        private void Update()
        {
            IInteractable best = FindBest();

            // Při změně cíle přehodíme zvýraznění (zhasne starý, rozsvítí nový).
            if (!ReferenceEquals(best, _current))
            {
                if (_current != null) _current.SetHighlighted(false);
                _current = best;
                if (_current != null) _current.SetHighlighted(true);
            }

            if (UIManager.Instance != null)
            {
                if (_current != null) UIManager.Instance.ShowPrompt($"[E]  {_current.GetInteractionPrompt()}");
                else UIManager.Instance.HidePrompt();
            }

            if (_current != null && Input.GetKeyDown(KeyCode.E))
                _current.Interact(_player);
        }

        private IInteractable FindBest()
        {
            IInteractable best = null;
            float bestDist = float.MaxValue;
            Vector3 origin = transform.position;
            Vector3 forward = transform.forward;

            for (int i = Registry.Count - 1; i >= 0; i--)
            {
                MonoBehaviour mb = Registry[i];
                if (mb == null) { Registry.RemoveAt(i); continue; } // úklid zničených

                var it = mb as IInteractable;
                if (it == null || !it.CanInteract()) continue;

                Vector3 to = mb.transform.position - origin;
                to.y = 0f;
                float dist = to.magnitude;
                if (dist > interactRange) continue;

                if (dist > 0.05f && Vector3.Dot(forward, to.normalized) < facingDotThreshold)
                    continue; // není před hráčem

                if (dist < bestDist) { bestDist = dist; best = it; }
            }

            return best;
        }
    }
}
