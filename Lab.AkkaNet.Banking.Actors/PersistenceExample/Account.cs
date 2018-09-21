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

        public static Props Create(int number, decimal initialBalance) => Props.Create(() => new Account(number, initialBalance));

        private int number;

        private decimal balance;


        public Account(int number, decimal initialBalance)
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
            // Logikprüfung hier, z. B. Dispo-Rahmen
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

        protected override ISnapshot GetSnapshot() 
        {
            return new AccountSnapshot()
            {
                Number = this.number,
                Balance = this.balance
            };
        }

        protected override void RestoreFromSnapshop(ISnapshot snapshot)
        {
            if (snapshot is AccountSnapshot accountSnapshot)
            {
                this.number = accountSnapshot.Number;
                this.balance = accountSnapshot.Balance;
            }
        }
    }

    public class AccountSnapshot : ISnapshot
    {

        public AccountSnapshot()
        {
            
        }

        public AccountSnapshot(int number, decimal balance)
        {
            Number = number;
            Balance = balance;
        }

        public int Number { get; set; }
        public decimal Balance { get; set; }

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
