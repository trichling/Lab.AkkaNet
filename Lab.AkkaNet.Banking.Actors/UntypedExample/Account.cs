using System;
using System.Collections.Generic;
using System.Text;
using Akka.Actor;

namespace Lab.AkkaNet.Banking.Actors.UntypedExample
{
    public class Account : UntypedActor
    {

        public static Props Create(int number, double initialBalance) => Props.Create(() => new Account(number, initialBalance));

        private int number;
        private double balance;

        public Account(int number, double initialBalance)
        {
            this.number = number;
            this.balance = initialBalance;
        }

        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case Deposit deposit:
                    balance += deposit.Amount;
                    break;
                case Withdraw withdraw:
                    balance -= withdraw.Amount;
                    break;
                case QueryBalance _:
                    Sender.Tell(balance);
                    break;
            }
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
        public Deposit(int number, double amount)
        {
            Number = number;
            Amount = amount;
        }
        public int Number { get; set; }
        public double Amount { get; set; }

    }

    public class Withdraw
    {

        public Withdraw(int number, double amount)
        {
            Number = number;
            Amount = amount;
        }

        public int Number { get; set; }
        public double Amount { get; set; }

    }
}
