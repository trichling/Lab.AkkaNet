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
            Context.System.EventStream.Subscribe(Self, typeof(AmountBlockedForTransfer));


            return message => {
                switch  (message)
                {
                    case InsufficiantBalance insufficiantBalance:
                        initiator.Tell(new TransferCanceled(transactionId));
                        Context.Stop(Self);
                    break;

                    case AmountBlockedForTransfer amountBlockedForTransfer:
                        destination.Account.Tell(new Deposit(destination.Number, transferBalance));
                        Become(WaitForDeposit());
                    break;
                }
            };
        }

        private UntypedReceive WaitForDeposit()
        {
            Context.System.EventStream.Subscribe(Self, typeof(AmountDeposited));
            var transferTimeoutTimer = Context.System.Scheduler.ScheduleTellOnceCancelable(TimeSpan.FromSeconds(60), Self, new TransferTimedOut(transactionId), Self);


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
                        source.Account.Tell(new WithdrawBlockedAmount(transactionId));
                        Become(WaitForWithdrawel());
                    break;
                }
            };
        }

        private UntypedReceive WaitForCompensation()
        {
            Context.System.EventStream.Subscribe(Self, typeof(BlockedAmountReleased));

            return message => 
            {
                switch (message)
                {
                    case BlockedAmountReleased amountReleased:
                        initiator.Tell(new TransferCanceled(transactionId));
                        Context.Stop(Self);
                    break;
                }
            };
        }

        private UntypedReceive WaitForWithdrawel()
        {
            Context.System.EventStream.Subscribe(Self, typeof(AmountWithdrawn));

            return message => 
            {
                switch (message)
                {
                    case AmountWithdrawn amountWithdrawn:
                        initiator.Tell(new TransferCompletedSuccesful(transactionId, source.Number, destination.Number, transferBalance));
                        Context.Stop(Self);
                    break;
                }
            };
        }
    }

   

}