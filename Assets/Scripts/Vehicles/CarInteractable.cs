using UnityEngine;
using Autobazar.Core;
using Autobazar.Interaction;
using Autobazar.Managers;
using Autobazar.Player;

namespace Autobazar.Vehicles
{
    /// <summary>
    /// Komponenta na autě. Drží data vozidla, vykresluje nad autem plovoucí ceduli
    /// (název, cena, status) a umožňuje hráči auto prohlédnout klávesou E.
    ///
    /// Ve Fázi 1 interakce jen ukáže informace. Prodej, servis a testovací jízda
    /// přibydou v dalších fázích.
    /// </summary>
    public class CarInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private CarData data = new CarData();
        [SerializeField] private float labelHeight = 2.2f;

        private TextMesh _label;

        public CarData Data => data;

        /// <summary>Nastaví data (volá WorldBuilder při stavbě scény).</summary>
        public void SetData(CarData newData)
        {
            data = newData;
            RefreshLabel();
        }

        private void OnEnable()
        {
            InteractionSystem.Register(this);
        }

        private void OnDisable()
        {
            InteractionSystem.Unregister(this);
        }

        private void Start()
        {
            BuildLabel();
            RefreshLabel();
        }

        private void BuildLabel()
        {
            if (_label != null) return;
            _label = TextFactory.Create("", transform, Vector3.up * labelHeight,
                fontSize: 60, characterSize: 0.1f, color: Color.white,
                anchor: TextAnchor.LowerCenter, billboard: true);
        }

        /// <summary>Aktualizuje text a barvu plovoucí cedule podle dat auta.</summary>
        public void RefreshLabel()
        {
            if (_label == null) return;
            _label.text = $"{data.carName}\n{data.price:n0} Kč\n[ {data.GetStatusText()} ]";
            _label.color = data.GetStatusColor();
        }

        // ---------- IInteractable ----------

        public string GetInteractionPrompt() => $"Prohlédnout: {data.carName}";

        public Transform GetTransform() => transform;

        public bool CanInteract() => !data.isSold;

        public void Interact(PlayerController player)
        {
            // Fáze 1: jen ukážeme detail auta. (Prodej přijde ve Fázi 2.)
            string info =
                $"{data.carName}  ({data.GetTypeText()})\n" +
                $"Cena: {data.price:n0} Kč\n" +
                $"Stav: {data.condition} %     Atraktivita: {data.attractiveness} %\n" +
                $"Status: {data.GetStatusText()}";

            if (UIManager.Instance != null)
                UIManager.Instance.ShowMessage(info, 4f);
        }
    }
}
