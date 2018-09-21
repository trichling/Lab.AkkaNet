using System;
using System.Collections.Generic;
using System.Text;

namespace Lab.AkkaNet.Banking.Vanilla
{
    public class Account
    {

        public Account(int number, decimal initialBalance)
        {
            Number = number;
            Balance = initialBalance;
        }

        public int Number { get; }
        public decimal Balance { get; private set; }

        public void Deposit(decimal amount)
        {
            Balance += amount;
        }

        public void Withdraw(decimal amount)
        {
            Balance -= amount;
        }

    }
}
