
namespace BankingSimulationSystemAPI.Model.DTO
{
    public class AccountRequest
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string AccountNumber {  get; set; }

        public decimal InitialBalance { get; set; }

    }
}
