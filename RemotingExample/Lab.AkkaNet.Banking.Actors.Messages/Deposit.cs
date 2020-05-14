using System;

namespace Lab.AkkaNet.Banking.Actors.Messages
{
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

}