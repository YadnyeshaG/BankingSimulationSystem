namespace BankingSimulationSystemAPI.Model.DTO
{
    public class TransactionResponse
    {
        public int TransactionId { get; set; }
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Description { get; set; }
        public TransactionType Type { get; set; }
    }
}
