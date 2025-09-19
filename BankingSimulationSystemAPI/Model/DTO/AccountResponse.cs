namespace BankingSimulationSystemAPI.Model.DTO
{
    public class AccountResponse
    {
        public int AccountId { get; set; }
        public string AccountNumber { get; set; }
        public decimal Balance { get; set; }

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        public List<TransactionResponse> Transactions { get; set; }
    }
}
