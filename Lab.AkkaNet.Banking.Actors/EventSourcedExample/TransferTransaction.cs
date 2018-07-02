using System;
using System.Collections.Generic;
using System.Text;
using Akka.Actor;
using Lab.AkkaNet.Banking.Actors.ActorBase;

namespace Lab.AkkaNet.Banking.Actors.EventSourcedExample
{
    public class TransferTransaction : UntypedActor
    {
        private readonly IActorRef initiator;
        private readonly (int Number, IActorRef Account) source;
        private readonly (int Number, IActorRef Account) destination;
        private readonly double transferBalance;
        private readonly Guid transactionId;

        public static Props Create(IActorRef initiator, Guid transactionId, (int Number, IActorRef Actor) source, (int Number, IActorRef Actor) destination, double transferAmount) => Props.Create(() => new TransferTransaction(initiator, transactionId, source, destination, transferAmount));

        public TransferTransaction(IActorRef initiator, Guid transactionId,  (int Number, IActorRef Actor) source,  (int Number, IActorRef Actor) destination, double transferBalance)
        {
            this.initiator = initiator;
            this.transactionId = transactionId;
            this.source = source;
            this.destination = destination;
            this.transferBalance = transferBalance;
        }

        protected override void OnReceive(object message)
        {
            
        }

        protected override void PreStart()
        {
            source.Account.Tell(new BlockAmounteForTransfer(source.Number, transferBalance, transactionId));
            Become(TryBlockAmountForTransfer());
        }

        private UntypedReceive TryBlockAmountForTransfer()
        {
            return message => {
                switch  (message)
                {
                    case InsufficiantBalance insufficiantBalance:
                        Report(new TransferCanceled(transactionId));
                        Context.Stop(Self);
                    break;

                    case AmountBlockedForTransfer amountBlockedForTransfer:
                        destination.Account.Tell(new Deposit(transactionId, destination.Number, transferBalance));
                        Become(WaitForDeposit());
                    break;
                }
            };
        }

        private void Report(TransferCanceled transferCanceled)
        {
            initiator.Tell(transferCanceled);
            Context.Parent.Tell(transferCanceled);
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
                        Report(new TransferCanceled(transactionId));
                        Context.Stop(Self);
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
                        Report(new TransferSucceeded(transactionId, source.Number, destination.Number, transferBalance));
                        Context.Stop(Self);
                    break;
                }
            };
        }

        private void Report(TransferSucceeded transferSucceeded)
        {
            initiator.Tell(transferSucceeded);
            Context.Parent.Tell(transferSucceeded);
        }
    }

   

}