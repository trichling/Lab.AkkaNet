using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Akka.Actor;
using Lab.AkkaNet.Banking.Actors.ActorBase;

namespace Lab.AkkaNet.Banking.Actors.EventSourcedExample
{
    public class Account : EventSourcedUntypedActor
    {

        public static Props Create(int number, decimal initialBalance) => Props.Create(() => new Account(number, initialBalance));

        private int number;

        private decimal balance;


        public Account(int number, decimal initialBalance)
        {
            this.number = number;
            this.balance = initialBalance;
        }

        // Deposit
       

        #region prebuild
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
#endregion
     
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
        public Deposit(Guid transactionId, int number, decimal amount)
        {
            TransactionId = transactionId;
            Number = number;
            Amount = amount;
        }

        public Guid TransactionId { get; }
        public int Number { get; set; }
        public decimal Amount { get; set; }

    }

    public class AmountDeposited : IEvent
    {
        public AmountDeposited(Guid transactionId, int number, decimal amount)
        {
            TransactionId = transactionId;
            Number = number;
            Amount = amount;
        }

        public Guid TransactionId { get; }
        public int Number { get; set; }
        public decimal Amount { get; set; }

    }

    public class Withdraw
    {

        public Withdraw(Guid transactionId, int number, decimal amount)
        {
            TransactionId = transactionId;
            Number = number;
            Amount = amount;
        }

        public Guid TransactionId { get; }
        public int Number { get;  }
        public decimal Amount { get;  }

    }

    public class AmountWithdrawn : IEvent
    {

        public AmountWithdrawn(Guid transactionId, int number, decimal amount)
        {
            TransactionId = transactionId;
            Number = number;
            Amount = amount;
        }

        public Guid TransactionId { get; }
        public int Number { get;  }
        public decimal Amount { get;  }

    }
}
