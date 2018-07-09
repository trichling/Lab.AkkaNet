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
        private HashSet<Guid> openTransfers;
        private int succeededTransfers;
        private int canceledTransfers;

        public Bank(string name)
        {
            this.name = name;
            this.openTransfers = new HashSet<Guid>();
        }

        public void Handle(QueryAccountBalance queryAccountBalance)
        {
            var account = Context.Child($"Account-{queryAccountBalance.Number}");
            account.Forward(new QueryBalance(queryAccountBalance.Number));
        }

        public void Handle(Audit audit)
        {
            Sender.Tell(new AuditResult(this.openTransfers.Count, this.succeededTransfers, this.canceledTransfers));
        }

        public void Handle(Open open)
        {
            Causes(new AccountOpened(open.Number, open.InitialBalance));
        }

        public void Apply(AccountOpened accountOpened)
        {
            var account = Context.Child($"Account-{accountOpened.Number}");
            if (account == ActorRefs.Nobody)
                Context.ActorOf(Account.Create(accountOpened.Number, accountOpened.InitialBalance), $"Account-{accountOpened.Number}");
        }

        public void Handle(Transfer transfer)
        {
            var sourceAccount = Context.Child($"Account-{transfer.SourceAccountNumber}");
            var targetAccount = Context.Child($"Account-{transfer.TargetAccountNumber}");

            var transactionId = Guid.NewGuid();
            var transferTransaction = Context.ActorOf(TransferTransaction.Create(Sender, transactionId, (transfer.SourceAccountNumber, sourceAccount), (transfer.TargetAccountNumber, targetAccount), transfer.Amount), $"Transaction-{transactionId}");
            transferTransaction.Tell(transfer);
            Causes(new TransferStarted(transactionId, transfer.SourceAccountNumber, transfer.TargetAccountNumber, transfer.Amount));
        }

        public void Apply(TransferStarted startedTransfer)
        {
            openTransfers.Add(startedTransfer.TransactionId);
        }

        public void Handle(TransferSucceeded successfulTransfer)
        {
            Causes(successfulTransfer);
        }

        public void Apply(TransferSucceeded successfulTransfer)
        {
            if (openTransfers.Remove(successfulTransfer.TransactionId))
                succeededTransfers++;
        }

        public void Handle(TransferCanceled canceledTransfer)
        {
            Causes(canceledTransfer);
        }

        public void Apply(TransferCanceled canceledTransfer)
        {
            if (openTransfers.Remove(canceledTransfer.TransactionId))
                canceledTransfers++;
        }

       

    }

    public class Audit
    {

    }

    public class AuditResult
    {
        
        public AuditResult(int openTransfers, int succeededTransfers, int canceledTransfers)
        {
            OpenTransfers = openTransfers;
            SucceededTransfers = succeededTransfers;
            CanceledTransfers = canceledTransfers;
        }

        public int OpenTransfers { get; }
        public int SucceededTransfers { get; }
        public int CanceledTransfers { get; }

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

    public class TransferStarted
    {

        public TransferStarted(Guid transactionId, int sourceAccountNumber, int targetAccountNumber, double amount)
        {
            TransactionId = transactionId;
            SourceAccountNumber = sourceAccountNumber;
            TargetAccountNumber = targetAccountNumber;
            Amount = amount;
        }

        public Guid TransactionId { get; }
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

    public class TransferSucceeded
    {

        public TransferSucceeded(Guid transactionId, int sourceAccountNumber, int targetAccountNumber, double amount)
        {
            TransactionId = transactionId;
            SourceAccountNumber = sourceAccountNumber;
            TargetAccountNumber = targetAccountNumber;
            Amount = amount;
        }

        public Guid TransactionId { get; }
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
