using System;
using UnityEngine;

namespace Autobazar.Vehicles
{
    /// <summary>
    /// Data jednoho vozidla. Je to obyčejná serializovatelná třída,
    /// takže ji uvidíš a můžeš upravovat přímo v Inspectoru na komponentě CarInteractable.
    /// </summary>
    [Serializable]
    public class CarData
    {
        [Header("Základní údaje")]
        public string carName = "Neznámé auto";
        public CarType type = CarType.Levne;

        [Tooltip("Cena v Kč")]
        public int price = 50000;

        [Header("Stav vozidla")]
        [Range(0, 100)] public int condition = 80;       // technický stav v %
        [Range(0, 100)] public int attractiveness = 50;  // jak je auto lákavé

        [Header("Stavové vlajky")]
        public bool needsService = false;  // potřebuje servis
        public bool isReserved = false;    // rezervováno zákazníkem
        public bool isSold = false;        // už prodáno

        /// <summary>Text statusu pro plovoucí ceduli a UI.</summary>
        public string GetStatusText()
        {
            if (isSold) return "PRODÁNO";
            if (isReserved) return "Rezervováno";
            if (needsService) return "Potřebuje servis";
            return "Na prodej";
        }

        /// <summary>Barva statusu (zelená = ok, oranžová = servis, atd.).</summary>
        public Color GetStatusColor()
        {
            if (isSold) return new Color(0.5f, 0.5f, 0.5f);
            if (isReserved) return new Color(0.3f, 0.6f, 1f);
            if (needsService) return new Color(1f, 0.6f, 0.1f);
            return new Color(0.4f, 1f, 0.4f);
        }

        /// <summary>Lidsky čitelný název typu auta.</summary>
        public string GetTypeText()
        {
            switch (type)
            {
                case CarType.Levne: return "Levné";
                case CarType.Sportovni: return "Sportovní";
                case CarType.Rodinne: return "Rodinné";
                case CarType.Pracovni: return "Pracovní";
                default: return type.ToString();
            }
        }
    }
}
