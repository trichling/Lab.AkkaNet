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

        public void Handle(Open open)
        {
            Causes(new AccountOpened(open.Number, open.InitialBalance));
        }

        public void Apply(AccountOpened accountOpened)
        {
            var newAccount = Context.ActorOf(Account.Create(accountOpened.Number, accountOpened.InitialBalance), $"Account-{accountOpened.Number}");
        }

        public void Handle(Transfer transfer)
        {
            var sourceAccount = Context.Child($"Account-{transfer.SourceAccountNumber}");
            var targetAccount = Context.Child($"Account-{transfer.TargetAccountNumber}");

            Context.ActorOf(TransferTransaction.Create(Self, Guid.NewGuid(), (transfer.SourceAccountNumber, sourceAccount), (transfer.TargetAccountNumber, targetAccount), transfer.Amount));
        }

        public void Apply(TransferCompletedSuccesful transferCompletedSuccesful)
        {
            
        }

        public void Handle(QueryAccountBalance queryAccountBalance)
        {
            var account = Context.Child($"Account-{queryAccountBalance.Number}");
            account.Forward(new QueryBalance(queryAccountBalance.Number));

            //account.Tell(new QueryBalance(queryAccountBalance.Number), Self);
            //account.Tell(new QueryBalance(queryAccountBalance.Number), Sender);
            //var amount = account.Ask<double>(new QueryBalance(queryAccountBalance.Number)).Result;
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

    public class Transfer
    {

        public Transfer(int sourceAccountNumber, int targetAccountNumber, double amount)
        {
            SourceAccountNumber = sourceAccountNumber;
            TargetAccountNumber = targetAccountNumber;
            Amount = amount;
        }
        public int SourceAccountNumber { get; }
        public int TargetAccountNumber { get; }
        public double Amount { get; }
    }

    public class TransferCanceled
    {
        public TransferCanceled(Guid transactionId)
        {
            TransactionId = transactionId;
        }

        public Guid TransactionId { get; }
    }

    public class TransferTimedOut
    {

        public TransferTimedOut(Guid transactionId)
        {
            TransactionId = transactionId;
        }

        public Guid TransactionId { get; }
    }

    public class TransferCompletedSuccesful
    {

        public TransferCompletedSuccesful(Guid transactionId, int sourceAccountNumber, int targetAccountNumber, double amount)
        {
            SourceAccountNumber = sourceAccountNumber;
            TargetAccountNumber = targetAccountNumber;
            Amount = amount;
        }
        public int SourceAccountNumber { get; }
        public int TargetAccountNumber { get; }
        public double Amount { get; }
    }

    public class Open
    {
        
        public Open(int number, double initialBalance)
        {
            Number = number;
            InitialBalance = initialBalance;
        }

        public int Number { get; }
        public double InitialBalance { get; }

    }

    public class AccountOpened
    {
        
        public AccountOpened(int number, double initialBalance)
        {
            Number = number;
            InitialBalance = initialBalance;
        }

        public int Number { get; }
        public double InitialBalance { get; }

    }
}
