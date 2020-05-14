using System;
using Lab.AkkaNet.Banking.Actors.Messages.ActorBase;

namespace Lab.AkkaNet.Banking.Actors.Messages
{
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