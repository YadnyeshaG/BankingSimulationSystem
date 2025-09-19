namespace BankingSimulationSystemAPI.Model
{
    public class Transaction
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public Account? Account { get; set; }
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
        public string Description { get; set; } = string.Empty;

        public TransactionType Type { get; set; }



    }
}
