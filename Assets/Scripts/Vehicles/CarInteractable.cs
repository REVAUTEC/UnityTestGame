using UnityEngine;
using Autobazar.Core;
using Autobazar.Interaction;
using Autobazar.Managers;
using Autobazar.Player;
using Autobazar.UI;

namespace Autobazar.Vehicles
{
    /// <summary>
    /// Komponenta na autě. Drží data vozidla, vykresluje nad autem plovoucí ceduli
    /// a umožňuje hráči auto prohlédnout klávesou E. Když se na auto hráč dívá,
    /// auto se lehce rozsvítí a cedule se zvětší (vizuální zpětná vazba).
    /// </summary>
    public class CarInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private CarData data = new CarData();
        [SerializeField] private float labelHeight = 2.35f;

        private TextMesh _label;
        private Transform _labelTransform;
        private Vector3 _labelBaseScale;
        private Material _paintMaterial;     // lak karoserie (pro rozsvícení)
        private bool _highlighted;

        public CarData Data => data;

        public void SetData(CarData newData)
        {
            data = newData;
            RefreshLabel();
        }

        private void OnEnable() => InteractionSystem.Register(this);
        private void OnDisable() => InteractionSystem.Unregister(this);

        private void Start()
        {
            // Najdeme materiál laku, abychom uměli auto rozsvítit.
            var body = transform.Find("Body");
            if (body != null)
            {
                var r = body.GetComponent<Renderer>();
                if (r != null) _paintMaterial = r.sharedMaterial;
            }

            BuildLabel();
            RefreshLabel();
        }

        private void BuildLabel()
        {
            if (_label != null) return;
            _label = TextFactory.Create("", transform, Vector3.up * labelHeight,
                fontSize: 60, characterSize: 0.1f, color: Color.white,
                anchor: TextAnchor.LowerCenter, billboard: true);
            _labelTransform = _label.transform;
            _labelBaseScale = _labelTransform.localScale;
        }

        public void RefreshLabel()
        {
            if (_label == null) return;
            _label.text = $"{data.carName}\n{data.price:n0} Kč\n[ {data.GetStatusText()} ]";
            _label.color = data.GetStatusColor();
        }

        // ---------- IInteractable ----------

        public string GetInteractionPrompt() => $"Servis: {data.carName}";
        public Transform GetTransform() => transform;
        public bool CanInteract() => !data.isSold;

        public void SetHighlighted(bool on)
        {
            _highlighted = on;

            // Rozsvícení laku.
            if (_paintMaterial != null)
            {
                Color glow = on ? data.GetStatusColor() * 0.35f : Color.black;
                MaterialFactory.SetEmission(_paintMaterial, glow);
            }

            // Zvětšení cedule.
            if (_labelTransform != null)
                _labelTransform.localScale = _labelBaseScale * (on ? 1.25f : 1f);
        }

        public void Interact(PlayerController player)
        {
            // Otevře servisní menu auta (oprava, mytí, leštění…).
            if (CarServiceUI.Instance != null) CarServiceUI.Instance.Open(this);
        }
    }
}
