using System;
using System.Collections.Generic;
using System.Text;

namespace Lab.AkkaNet.Banking.Vanilla
{
    public class Account
    {

        public Account(int number, double initialBalance)
        {
            Number = number;
            Balance = initialBalance;
        }

        public int Number { get; }
        public double Balance { get; private set; }

        public void Deposit(double amount)
        {
            Balance += amount;
        }

        public void Withdraw(double amount)
        {
            Balance -= amount;
        }

    }
}
