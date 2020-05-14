using System;
using Lab.AkkaNet.Banking.Actors.Messages.ActorBase;

namespace Lab.AkkaNet.Banking.Actors.Messages
{
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

}