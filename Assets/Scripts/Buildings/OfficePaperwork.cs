using UnityEngine;
using Autobazar.Core;
using Autobazar.Interaction;
using Autobazar.Managers;
using Autobazar.Player;

namespace Autobazar.Buildings
{
    /// <summary>
    /// Počítač v kanceláři. Když je rozjednaná smlouva, hráč u něj stiskne E,
    /// proběhne „příprava papírů" (progress bar) a obchod se dokončí.
    /// </summary>
    public class OfficePaperwork : MonoBehaviour, IInteractable
    {
        [SerializeField] private float paperworkDuration = 5f;

        private Material _screenMaterial;
        private Color _screenBaseEmission = new Color(0.2f, 0.5f, 0.9f);

        private void OnEnable() => InteractionSystem.Register(this);
        private void OnDisable() => InteractionSystem.Unregister(this);

        private void Start()
        {
            var screen = transform.Find("Screen");
            if (screen != null)
            {
                var r = screen.GetComponent<Renderer>();
                if (r != null) _screenMaterial = r.sharedMaterial;
            }
            SetHighlighted(false);
        }

        public string GetInteractionPrompt()
            => DealManager.HasPendingDeal ? "Připravit smlouvu" : "Počítač (žádná smlouva)";

        public Transform GetTransform() => transform;

        public bool CanInteract() => true;

        public void SetHighlighted(bool on)
        {
            if (_screenMaterial != null)
                MaterialFactory.SetEmission(_screenMaterial, _screenBaseEmission * (on ? 1.6f : 0.7f));
        }

        public void Interact(PlayerController player)
        {
            if (!DealManager.HasPendingDeal)
            {
                if (UIManager.Instance != null)
                    UIManager.Instance.ShowMessage("Teď nemáš žádnou rozjednanou smlouvu.\nNejdřív se domluv se zákazníkem (E).", 3.5f);
                return;
            }

            if (UIManager.Instance != null)
                UIManager.Instance.ShowProgress("Příprava smlouvy", paperworkDuration, DealManager.CompletePendingDeal);
            else
                DealManager.CompletePendingDeal();
        }
    }
}
