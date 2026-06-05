using UnityEngine;
using Autobazar.Vehicles;

namespace Autobazar.Managers
{
    public enum RepairType { Wash, Polish, Engine, Brakes }

    /// <summary>
    /// Servisní úkony: definuje cenu, dobu a efekt jednotlivých oprav a aplikuje je na auto.
    /// Platbu řeší volající (CarServiceUI) přes EconomyManager.
    /// </summary>
    public static class ServiceManager
    {
        public struct RepairInfo
        {
            public string Name;
            public int Cost;
            public string Effect;
        }

        public static RepairInfo GetInfo(RepairType type)
        {
            switch (type)
            {
                case RepairType.Wash:   return new RepairInfo { Name = "Umýt auto",      Cost = 800,  Effect = "+10 atraktivita" };
                case RepairType.Polish: return new RepairInfo { Name = "Vyleštit lak",   Cost = 1500, Effect = "+18 atraktivita" };
                case RepairType.Engine: return new RepairInfo { Name = "Oprava motoru",  Cost = 4000, Effect = "+20 stav" };
                case RepairType.Brakes: return new RepairInfo { Name = "Oprava brzd",    Cost = 2500, Effect = "+12 stav" };
                default: return new RepairInfo { Name = "?", Cost = 0, Effect = "" };
            }
        }

        public static float GetDuration(RepairType type) => 2f;

        /// <summary>Aplikuje efekt opravy (platba už proběhla).</summary>
        public static void Apply(RepairType type, CarInteractable car)
        {
            if (car == null) return;
            var d = car.Data;

            switch (type)
            {
                case RepairType.Wash:   d.attractiveness = Clamp(d.attractiveness + 10); break;
                case RepairType.Polish: d.attractiveness = Clamp(d.attractiveness + 18); break;
                case RepairType.Engine: d.condition = Clamp(d.condition + 20); break;
                case RepairType.Brakes: d.condition = Clamp(d.condition + 12); break;
            }

            // Když je auto v dobrém stavu, servis už nepotřebuje.
            if (d.condition >= 70) d.needsService = false;

            car.RefreshLabel();
        }

        private static int Clamp(int v) => Mathf.Clamp(v, 0, 100);
    }
}
