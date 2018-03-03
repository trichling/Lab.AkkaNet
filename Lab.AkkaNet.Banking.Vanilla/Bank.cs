using System;
using System.Collections.Generic;
using System.Text;

namespace Lab.AkkaNet.Banking.Vanilla
{
    public class Bank
    {
        private readonly Dictionary<int, Account> _accounts;

        public Bank(string name)
        {
            Name = name;
            _accounts = new Dictionary<int, Account>();
        }

        public string Name { get; }

        public Account Open(int number, double initialBalance)
        {
            var account = new Account(number, initialBalance);
            _accounts.Add(account.Number, account);
            return account;
        }

        public void Transfer(int sourceAccountNumber, int targetAccountNumber, double amount)
        {
            var sourceAccount = _accounts[sourceAccountNumber];
            var targetAccount = _accounts[targetAccountNumber];

            sourceAccount.Withdraw(amount);
            targetAccount.Deposit(amount);
        }
    }
}
