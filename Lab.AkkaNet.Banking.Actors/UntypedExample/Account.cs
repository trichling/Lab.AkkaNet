using System;
using System.Collections.Generic;
using System.Text;
using Akka.Actor;

namespace Lab.AkkaNet.Banking.Actors.UntypedExample
{
    public class Account : UntypedActor
    {

        public static Props Create(int number, decimal initialBalance) => Props.Create(() => new Account(number, initialBalance));

        private int number;
        private decimal balance;

        public Account(int number, decimal initialBalance)
        {
            this.number = number;
            this.balance = initialBalance;
        }

       
    }

   

    public class QueryBalance
    {

        public QueryBalance(int number)
        {
            Number = number;
        }
        public int Number { get; set; }

    }

    public class Deposit
    {
        public Deposit(int number, decimal amount)
        {
            Number = number;
            Amount = amount;
        }
        public int Number { get; set; }
        public decimal Amount { get; set; }

    }

    public class Withdraw
    {

        public Withdraw(int number, decimal amount)
        {
            Number = number;
            Amount = amount;
        }

        public int Number { get; set; }
        public decimal Amount { get; set; }

    }
}
