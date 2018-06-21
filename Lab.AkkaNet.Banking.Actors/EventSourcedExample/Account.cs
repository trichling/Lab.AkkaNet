using System;
using System.Collections.Generic;
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

        public void Handle(BlockAmounteForTransfer blockAmounteForTransfer)
        {
            // check number

            if (balance > blockAmounteForTransfer.Amount)
                Causes(new AmountBlockedForTransfer(blockAmounteForTransfer.Amount, blockAmounteForTransfer.TransactionId));
            else
                Sender.Tell(new InsufficiantBalance(number, blockAmounteForTransfer.Amount, balance));
        }

        public void Apply(AmountBlockedForTransfer amountBlockedForTransfer)
        {
            outstandingTransfers.Add(amountBlockedForTransfer.TransactionId, amountBlockedForTransfer.Amount);
        }

        public void Handle(WithdrawBlockedAmount withdraw)
        {
            var amount = outstandingTransfers[withdraw.TransactionId];
            Causes(new BlockedAmountWithdrawn(withdraw.TransactionId, amount));
        }

        public void Apply(BlockedAmountWithdrawn amountBlockedForTransfer)
        {
            balance -= amountBlockedForTransfer.WithdrawnAmount;
            outstandingTransfers.Remove(amountBlockedForTransfer.TransactionId);
        }

        public void Handle(Deposit deposit)
        {
            Causes(new AmountDeposited(number, deposit.Amount));
        }

        public void Apply(AmountDeposited amountDeposited)
        {
            balance += amountDeposited.Amount;
        }

        public void Handle(Withdraw withdraw)
        {
            if (balance < withdraw.Amount)
            {
                Sender.Tell(new InsufficiantBalance(number, withdraw.Amount, balance));
            }

            Causes(new AmountWithdrawn(number, withdraw.Amount));
        }

        public void Apply(AmountWithdrawn amountWithdrawn)
        {
            balance -= amountWithdrawn.Amount;
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

    public class WithdrawBlockedAmount
    {
        public WithdrawBlockedAmount( Guid transactionId)
        {
            TransactionId = transactionId;
        }

        public Guid TransactionId { get; }
    }

    public class BlockedAmountWithdrawn
    {
        public BlockedAmountWithdrawn(Guid transactionId, double withdrawnAmount)
        {
            TransactionId = transactionId;
            WithdrawnAmount = withdrawnAmount;
        }

        public Guid TransactionId { get; }
        public double WithdrawnAmount { get; }
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
        public Deposit(int number, double amount)
        {
            Number = number;
            Amount = amount;
        }
        public int Number { get; set; }
        public double Amount { get; set; }

    }

    public class AmountDeposited
    {
        public AmountDeposited(int number, double amount)
        {
            Number = number;
            Amount = amount;
        }

        public int Number { get; set; }
        public double Amount { get; set; }

    }

    public class Withdraw
    {

        public Withdraw(int number, double amount)
        {
            Number = number;
            Amount = amount;
        }

        public int Number { get;  }
        public double Amount { get;  }

    }

    public class AmountWithdrawn
    {

        public AmountWithdrawn(int number, double amount)
        {
            Number = number;
            Amount = amount;
        }

        public int Number { get;  }
        public double Amount { get;  }

    }
}
