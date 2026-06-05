using Autobazar.People;
using Autobazar.Vehicles;

namespace Autobazar.Managers
{
    /// <summary>
    /// Řídí obchod jako proces: zákazník nejdřív souhlasí (Agree) a auto se rezervuje,
    /// pak hráč připraví smlouvu v kanceláři (CompletePendingDeal) a teprve tím se prodej
    /// dokončí (peníze, prodaná auta, reputace, zákazník odejde spokojený).
    /// Najednou může být rozjednaná jen jedna smlouva.
    /// </summary>
    public static class DealManager
    {
        public static Customer PendingCustomer { get; private set; }
        public static CarInteractable PendingCar { get; private set; }
        public static bool HasPendingDeal => PendingCustomer != null && PendingCar != null;

        public struct Result
        {
            public bool Success;
            public string Message;
        }

        /// <summary>Zákazník souhlasí s autem → rezervace a čekání na smlouvu.</summary>
        public static Result Agree(Customer customer, CarInteractable car)
        {
            var data = car.Data;

            if (data.isSold) return Fail($"{data.carName} je už prodané.");
            if (data.isReserved) return Fail($"{data.carName} je už rezervované.");
            if (HasPendingDeal) return Fail("Nejdřív dokonči současnou smlouvu v kanceláři.");
            if (data.type != customer.DesiredType)
                return Fail($"„{data.GetTypeText()} auto? To nehledám, chci {CarData.TypeText(customer.DesiredType)}.\"");

            data.isReserved = true;
            car.RefreshLabel();
            PendingCustomer = customer;
            PendingCar = car;
            customer.MarkReserved();

            if (TaskManager.Instance != null)
                TaskManager.Instance.SetTask("Zákazník souhlasí! Dojdi do kanceláře k počítači a stiskni E (připrav smlouvu).");

            return new Result
            {
                Success = true,
                Message = $"Zákazník souhlasí s koupí: {data.carName}!\nPřiprav smlouvu v kanceláři (počítač u kanceláře, klávesa E)."
            };
        }

        /// <summary>Dokončí rozjednaný obchod – volá kancelář po „papírování".</summary>
        public static void CompletePendingDeal()
        {
            if (!HasPendingDeal) return;

            var car = PendingCar;
            var customer = PendingCustomer;
            var data = car.Data;
            int price = data.price;
            int repGain = data.needsService ? 3 : 6;

            data.isReserved = false;
            data.isSold = true;
            car.RefreshLabel();

            if (EconomyManager.Instance != null)
            {
                EconomyManager.Instance.AddMoney(price);
                EconomyManager.Instance.RegisterCarSold();
            }
            if (ReputationManager.Instance != null)
                ReputationManager.Instance.ChangeReputation(repGain);

            if (customer != null) customer.CompleteSale();

            if (UIManager.Instance != null)
                UIManager.Instance.ShowMessage($"SMLOUVA HOTOVÁ – {data.carName} prodáno za {price:n0} Kč!  +{repGain} reputace", 4f);
            if (TaskManager.Instance != null)
                TaskManager.Instance.SetTask("Hotovo! Počkej na dalšího zákazníka u vstupu.");

            PendingCustomer = null;
            PendingCar = null;
        }

        /// <summary>Bezpečně zruší rozjednaný obchod (např. když zákazník odejde).</summary>
        public static void CancelPendingDealFor(Customer customer)
        {
            if (PendingCustomer != customer) return;
            if (PendingCar != null) { PendingCar.Data.isReserved = false; PendingCar.RefreshLabel(); }
            PendingCustomer = null;
            PendingCar = null;
        }

        private static Result Fail(string message) => new Result { Success = false, Message = message };
    }
}
