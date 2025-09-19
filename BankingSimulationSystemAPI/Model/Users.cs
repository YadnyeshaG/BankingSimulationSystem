namespace BankingSimulationSystemAPI.Model
{
    public class Users
    {
        public int ID { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string UserName { get; set; }

        public string PasswordHash { get; set; }


        public Account? Account { get; set; }
    }
}
