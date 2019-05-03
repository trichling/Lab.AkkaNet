using System.Collections.Generic;

namespace Lab.AkkaNet.Banking.Actors.PersistenceExample
{
    public class AccountBalanceDatabase
    {

        private Dictionary<int, decimal> _accountBalances = new Dictionary<int, decimal>();

        public decimal Select(int accountNumber)
        {
            return _accountBalances[accountNumber];
        }

        public void Insert(int accountNumber, decimal balance)
        {
            _accountBalances.Add(accountNumber, balance);
        }

        public void Update(int accountNumber, decimal balance)
        {
            _accountBalances[accountNumber] = balance;
        }

    }

}