using System;
using Lab.AkkaNet.Banking.Actors.Messages.ActorBase;

namespace Lab.AkkaNet.Banking.Actors.Messages
{

    public class Withdraw
    {

        public Withdraw(Guid transactionId, int number, decimal amount)
        {
            TransactionId = transactionId;
            Number = number;
            Amount = amount;
        }

        public Guid TransactionId { get; }
        public int Number { get; }
        public decimal Amount { get; }

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
        public int Number { get; }
        public decimal Amount { get; }

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