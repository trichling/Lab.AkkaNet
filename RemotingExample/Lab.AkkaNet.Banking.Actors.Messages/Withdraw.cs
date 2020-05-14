using System;

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

}