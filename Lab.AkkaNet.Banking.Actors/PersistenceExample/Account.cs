using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Akka.Actor;
using Lab.AkkaNet.Banking.Actors.ActorBase;

namespace Lab.AkkaNet.Banking.Actors.PersistenceExample
{
    public class Account : EventSourcedUntypedPresistentActor
    {

        public static Props Create(int number, double initialBalance) => Props.Create(() => new Account(number, initialBalance));

        private int number;

        private double balance;


        public Account(int number, double initialBalance)
        {
            this.number = number;
            this.balance = initialBalance;
        }

        public override string PersistenceId => $"Account-{number}";

        public void Handle(Deposit deposit)
        {
            Causes(new AmountDeposited(deposit.TransactionId, number, deposit.Amount));
        }

        public void Apply(AmountDeposited amountDeposited)
        {
            balance += amountDeposited.Amount;
        }

        public void Handle(Withdraw withdraw)
        {
            Causes(new AmountWithdrawn(withdraw.TransactionId, number, withdraw.Amount));
        }


        public void Apply(AmountWithdrawn amountWithdrawn)
        {
            balance -= amountWithdrawn.Amount;
        }
        
        public void Handle(QueryBalance queryBalance)
        {
            Sender.Tell(balance);
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
        public Deposit(Guid transactionId, int number, double amount)
        {
            TransactionId = transactionId;
            Number = number;
            Amount = amount;
        }

        public Guid TransactionId { get; }
        public int Number { get; set; }
        public double Amount { get; set; }

    }

    public class AmountDeposited : IEvent
    {
        public AmountDeposited(Guid transactionId, int number, double amount)
        {
            TransactionId = transactionId;
            Number = number;
            Amount = amount;
        }

        public Guid TransactionId { get; }
        public int Number { get; set; }
        public double Amount { get; set; }

    }

    public class Withdraw
    {

        public Withdraw(Guid transactionId, int number, double amount)
        {
            TransactionId = transactionId;
            Number = number;
            Amount = amount;
        }

        public Guid TransactionId { get; }
        public int Number { get;  }
        public double Amount { get;  }

    }

    public class AmountWithdrawn : IEvent
    {

        public AmountWithdrawn(Guid transactionId, int number, double amount)
        {
            TransactionId = transactionId;
            Number = number;
            Amount = amount;
        }

        public Guid TransactionId { get; }
        public int Number { get;  }
        public double Amount { get;  }

    }
}
