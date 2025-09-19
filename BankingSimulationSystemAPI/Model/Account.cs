namespace BankingSimulationSystemAPI.Model
{
    public class Account
    {
        public int ID { get; set; }

        public int UserID { get; set; }

        public Users? User { get; set; }

        public string AccountNumber { get; set; }

        public decimal Balance { get; set; }

        public List<Transaction> Transactions { get; set; }

    }
}
