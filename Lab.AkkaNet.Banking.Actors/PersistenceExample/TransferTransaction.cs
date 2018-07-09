using System;
using System.Collections.Generic;
using System.Text;
using Akka.Actor;
using Lab.AkkaNet.Banking.Actors.ActorBase;

namespace Lab.AkkaNet.Banking.Actors.PersistenceExample
{
    public class TransferTransaction : EventSourcedUntypedPresistentActor
    {
        private IActorRef initiator;
        private (int Number, IActorRef Account) source;
        private (int Number, IActorRef Account) destination;
        private double transferBalance;
        private  Guid transactionId;

        private string transferState;

        public override string PersistenceId => $"Transaction-{transactionId.ToString()}";

        public static Props Create(IActorRef initiator, Guid transactionId, (int Number, IActorRef Actor) source, (int Number, IActorRef Actor) destination, double transferAmount) => Props.Create(() => new TransferTransaction(initiator, transactionId, source, destination, transferAmount));

      public TransferTransaction(IActorRef initiator, Guid transactionId, (int Number, IActorRef Actor) source,  (int Number, IActorRef Actor) destination, double transferBalance)
        {
            this.initiator = initiator;
            this.transactionId = transactionId;
            this.source = source;
            this.destination = destination;
            this.transferBalance = transferBalance;
        }

        public void Handle(Transfer transfer)
        {
            source.Account.Tell(new BlockAmounteForTransfer(source.Number, transferBalance, transactionId));
            Become(TryBlockAmountForTransfer());
            Causes(new TransferStarted(transactionId, transfer.SourceAccountNumber, transfer.TargetAccountNumber, transfer.Amount));
        }

        public void Apply(TransferStarted transferStarted)
        {
            this.source.Number = transferStarted.SourceAccountNumber;
            this.destination.Number = transferStarted.TargetAccountNumber;
            this.transactionId = transferStarted.TransactionId;
            this.transferBalance = transferStarted.Amount;
        }

        private UntypedReceive TryBlockAmountForTransfer()
        {
            return message => {
                switch  (message)
                {
                    case InsufficiantBalance insufficiantBalance:
                        Causes(new TransferCanceled(transactionId));
                        Context.Stop(Self);
                    break;

                    case AmountBlockedForTransfer amountBlockedForTransfer:
                        destination.Account.Tell(new Deposit(transactionId, destination.Number, transferBalance));
                        Become(WaitForDeposit());
                    break;
                }
            };
        }

       

        private UntypedReceive WaitForDeposit()
        {
            var transferTimeoutTimer = Context.System.Scheduler.ScheduleTellOnceCancelable(TimeSpan.FromSeconds(1), Self, new TransferTimedOut(transactionId), Self);

            return message => 
            {
                switch (message)
                {
                    case TransferTimedOut timeout:
                        source.Account.Tell(new ReleaseBlockedAmount(transactionId));
                        Become(WaitForCompensation());
                    break;
                    case AmountDeposited amountDeposited:
                        transferTimeoutTimer.Cancel();
                        source.Account.Tell(new Withdraw(transactionId, source.Number, transferBalance));
                        Become(WaitForWithdrawel());
                    break;
                }
            };
        }

        private UntypedReceive WaitForCompensation()
        {
            return message => 
            {
                switch (message)
                {
                    case BlockedAmountReleased amountReleased when amountReleased.TransactionId == transactionId:
                        Causes(new TransferCanceled(transactionId));
                    break;
                }
            };
        }

        private UntypedReceive WaitForWithdrawel()
        {

            return message => 
            {
                switch (message)
                {
                    case AmountWithdrawn amountWithdrawn:
                        Causes(new TransferSucceeded(transactionId, source.Number, destination.Number, transferBalance));
                    break;
                }
            };
        }

        public void Apply(TransferCanceled transferCanceled)
        {
            if (initiator != null)
                initiator.Tell(transferCanceled);
            
            this.transferState = "Canceled";

            Context.Stop(Self);

        }

        public void Apply(TransferSucceeded transferSucceeded)
        {
            if (initiator != null)
                initiator.Tell(transferSucceeded);
            
            this.transferState = "Succeeded";

            Context.Stop(Self);

        }
    }

   

}