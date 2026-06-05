using Autobazar.People;
using Autobazar.Vehicles;

namespace Autobazar.Managers
{
    /// <summary>
    /// Vyhodnocení obchodu: porovná nabízené auto s přáním zákazníka a v případě
    /// shody provede prodej (peníze, prodaná auta, reputace) a zákazník odejde spokojený.
    /// </summary>
    public static class DealManager
    {
        public struct Result
        {
            public bool Success;
            public string Message;
        }

        public static Result TrySell(Customer customer, CarInteractable car)
        {
            var data = car.Data;

            if (data.isSold)
                return Fail($"{data.carName} je už prodané.");

            // Typ neodpovídá přání → zákazník odmítne (bez postihu, zkus jiné).
            if (data.type != customer.DesiredType)
                return Fail($"„{data.GetTypeText()} auto? To nehledám, chci {CarData.TypeText(customer.DesiredType)}.\"");

            // Shoda → prodej
            int price = data.price;
            int repGain = data.needsService ? 3 : 6;

            data.isSold = true;
            car.RefreshLabel();

            if (EconomyManager.Instance != null)
            {
                EconomyManager.Instance.AddMoney(price);
                EconomyManager.Instance.RegisterCarSold();
            }
            if (ReputationManager.Instance != null)
                ReputationManager.Instance.ChangeReputation(repGain);

            customer.CompleteSale();

            if (TaskManager.Instance != null)
                TaskManager.Instance.SetTask("Skvělé! Počkej na dalšího zákazníka u vstupu.");

            string note = data.needsService ? "\n(Auto chtělo do servisu – zákazník trochu váhal.)" : "";
            return new Result
            {
                Success = true,
                Message = $"PRODÁNO: {data.carName} za {price:n0} Kč!  +{repGain} reputace{note}"
            };
        }

        private static Result Fail(string message) => new Result { Success = false, Message = message };
    }
}
