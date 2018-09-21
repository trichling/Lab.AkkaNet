using System;
using System.Collections.Generic;
using System.Text;
using Akka.Actor;
using Lab.AkkaNet.Banking.Actors.ActorBase;

namespace Lab.AkkaNet.Banking.Actors.EventSourcedExample
{
    public class Bank : EventSourcedUntypedActor
    {
     public static Props Create(string name) => Props.Create(() => new Bank(name));

        private string name;

        public Bank(string name)
        {
            this.name = name;
        }

        public void Handle(QueryAccountBalance queryAccountBalance)
        {
            var account = Context.Child($"Account-{queryAccountBalance.Number}");
            account.Forward(new QueryBalance(queryAccountBalance.Number));
        }

        public void Handle(Open open)
        {
            if (AccountDoesNotExist(open.Number))
            {
                Causes(new AccountOpened(open.Number, open.InitialBalance));
                return;
            }

            // error - account alredy exists
        }

        private bool AccountDoesNotExist(int number)
        {
            var account = Context.Child($"Account-{number}");
            return account == ActorRefs.Nobody;
        }

        public void Apply(AccountOpened accountOpened)
        {
            Context.ActorOf(Account.Create(accountOpened.Number, accountOpened.InitialBalance), $"Account-{accountOpened.Number}");
        }

        public void Handle(Transfer transfer)
        {
            var sourceAccount = Context.Child($"Account-{transfer.SourceAccountNumber}");
            var targetAccount = Context.Child($"Account-{transfer.TargetAccountNumber}");

            var transactionId = Guid.NewGuid();
            sourceAccount.Tell(new Withdraw(transactionId, transfer.SourceAccountNumber, transfer.Amount));
            targetAccount.Tell(new Deposit(transactionId, transfer.TargetAccountNumber, transfer.Amount));
            Causes(new MoneyTransfered(transactionId, transfer.SourceAccountNumber, transfer.TargetAccountNumber, transfer.Amount));
        }
       
        public void Apply(MoneyTransfered successfulTransfer)
        {
           
        }


    }

    public class QueryAccountBalance
    {

        public QueryAccountBalance(int number)
        {
            Number = number;
        }

        public int Number { get; }

    }

    public class Open
    {
        
        public Open(int number, decimal initialBalance)
        {
            Number = number;
            InitialBalance = initialBalance;
        }

        public int Number { get; }
        public decimal InitialBalance { get; }

    }

    public class AccountOpened : IEvent
    {
        
        public AccountOpened(int number, decimal initialBalance)
        {
            Number = number;
            InitialBalance = initialBalance;
        }

        public int Number { get; }
        public decimal InitialBalance { get; }

    }

    public class Transfer
    {

        public Transfer(int sourceAccountNumber, int targetAccountNumber, decimal amount)
        {
            SourceAccountNumber = sourceAccountNumber;
            TargetAccountNumber = targetAccountNumber;
            Amount = amount;
        }

        public int SourceAccountNumber { get; }
        public int TargetAccountNumber { get; }
        public decimal Amount { get; }
    }

  
    public class MoneyTransfered : IEvent
    { 

        public MoneyTransfered(Guid transactionId, int sourceAccountNumber, int targetAccountNumber, decimal amount)
        {
            TransactionId = transactionId;
            SourceAccountNumber = sourceAccountNumber;
            TargetAccountNumber = targetAccountNumber;
            Amount = amount;
        }

        public Guid TransactionId { get; }
        public int SourceAccountNumber { get; }
        public int TargetAccountNumber { get; }
        public decimal Amount { get; }
    }


}
