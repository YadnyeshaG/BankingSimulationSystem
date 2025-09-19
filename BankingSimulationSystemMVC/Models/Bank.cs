namespace BankingSimulationSystemMVC.Models
{
    public class Account
    {
        public int AccountId { get; set; }
        public string AccountNumber { get; set; }
        public decimal Balance { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public List<Transaction> Transactions { get; set; } = new();
    }


    public enum TransactionType
    {
        Deposit = 0,
        Withdraw = 1
    }

    public class Transaction
    {
        public int TransactionId { get; set; }
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; } 
        public string Description { get; set; }
        public TransactionType Type { get; set; }
    }


    public class TransactionRequest
    {
        public decimal Amount { get; set; }
        public string Description { get; set; }
     }
}
