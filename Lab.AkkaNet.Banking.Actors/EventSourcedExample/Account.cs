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

        public static Props Create(int number, double initialBalance) => Props.Create(() => new Account(number, initialBalance));

        private int number;

        private double balance;

        private Dictionary<Guid, double> outstandingTransfers;

        public Account(int number, double initialBalance)
        {
            this.number = number;
            this.balance = initialBalance;
            this.outstandingTransfers = new Dictionary<Guid, double>();
        }

        public double AvailableBalance => balance - outstandingTransfers.Sum(t => t.Value);

        public void Handle(BlockAmounteForTransfer blockAmountForTransfer)
        {
            // check number

            if (AvailableBalance > blockAmountForTransfer.Amount)
            {
                Causes(new AmountBlockedForTransfer(blockAmountForTransfer.Amount, blockAmountForTransfer.TransactionId));
            }
            else
            {
                Sender.Tell(new InsufficiantBalance(number, blockAmountForTransfer.Amount, AvailableBalance));
            }
        }

        public void Apply(AmountBlockedForTransfer amountBlockedForTransfer)
        {
            outstandingTransfers.Add(amountBlockedForTransfer.TransactionId, amountBlockedForTransfer.Amount);
        }

     
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
            if (!IsWithdrawPartOfATransfer(withdraw.TransactionId))
            {
                if (AvailableBalance < withdraw.Amount)
                {
                    Sender.Tell(new InsufficiantBalance(number, withdraw.Amount, AvailableBalance));
                    return;
                }
            }

            Causes(new AmountWithdrawn(withdraw.TransactionId, number, withdraw.Amount));
        }


        public void Apply(AmountWithdrawn amountWithdrawn)
        {
            if (IsWithdrawPartOfATransfer(amountWithdrawn.TransactionId))
            {
                var amount = outstandingTransfers[amountWithdrawn.TransactionId];
                outstandingTransfers.Remove(amountWithdrawn.TransactionId);
                balance -= amount;
            }
            else
                balance -= amountWithdrawn.Amount;
        }
        
        private bool IsWithdrawPartOfATransfer(Guid transacionId)
        {
           return outstandingTransfers.ContainsKey(transacionId);
        }

        public void Handle(QueryBalance queryBalance)
        {
            Sender.Tell(balance);
        }

     
    }

    public class BlockAmounteForTransfer
    {
        public BlockAmounteForTransfer(int number, double amount, Guid transactionId)
        {
            Number = number;
            Amount = amount;
            TransactionId = transactionId;
        }

        public int Number { get; }
        public double Amount { get; }
        public Guid TransactionId { get; }
    }

    public class AmountBlockedForTransfer
    {
        public AmountBlockedForTransfer(double amount, Guid transactionId)
        {
            Amount = amount;
            TransactionId = transactionId;
        }
        
        public double Amount { get; }
        public Guid TransactionId { get; }
    }

    public class ReleaseBlockedAmount
    {
        public ReleaseBlockedAmount( Guid transactionId)
        {
            TransactionId = transactionId;
        }

        public Guid TransactionId { get; }
    }

    public class BlockedAmountReleased
    {
        public BlockedAmountReleased(Guid transactionId, double releasedAmount)
        {
            TransactionId = transactionId;
            ReleasedAmount = releasedAmount;
        }

        public Guid TransactionId { get; }
        public double ReleasedAmount { get; }
    }

    public class QueryBalance
    {
        public QueryBalance(int number)
        {
            Number = number;
        }
        public int Number { get; set; }

    }

    public class InsufficiantBalance
    {

        public InsufficiantBalance(int number, double requested, double balance)
        {
            Number = number;
            Requested = requested;
            Balance = balance;
        }
        public int Number { get; }
        public double Requested { get; }
        public double Balance { get; }
    }

    public class Deposit
    {
        public Deposit(Guid transactionId, int number, double amount)
        {
            TransactionId = transactionId;
            Number = number;
            Amount = amount;
        }

        public Guid TransactionId { get; }
        public int Number { get; set; }
        public double Amount { get; set; }

    }

    public class AmountDeposited
    {
        public AmountDeposited(Guid transactionId, int number, double amount)
        {
            TransactionId = transactionId;
            Number = number;
            Amount = amount;
        }

        public Guid TransactionId { get; }
        public int Number { get; set; }
        public double Amount { get; set; }

    }

    public class Withdraw
    {

        public Withdraw(Guid transactionId, int number, double amount)
        {
            TransactionId = transactionId;
            Number = number;
            Amount = amount;
        }

        public Guid TransactionId { get; }
        public int Number { get;  }
        public double Amount { get;  }

    }

    public class AmountWithdrawn
    {

        public AmountWithdrawn(Guid transactionId, int number, double amount)
        {
            TransactionId = transactionId;
            Number = number;
            Amount = amount;
        }

        public Guid TransactionId { get; }
        public int Number { get;  }
        public double Amount { get;  }

    }
}
