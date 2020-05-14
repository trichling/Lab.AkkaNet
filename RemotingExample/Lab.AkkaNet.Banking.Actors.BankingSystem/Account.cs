using Akka.Actor;
using Lab.AkkaNet.Banking.Actors.Messages;
using Lab.AkkaNet.Banking.Actors.Messages.ActorBase;

namespace Lab.AkkaNet.Banking.Actors.BankingSystem
{
    public class Account : EventSourcedUntypedActor
    {

        public static Props Create(int number, decimal initialBalance) => Props.Create(() => new Account(number, initialBalance));

        private int number;

        private decimal balance;


        public Account(int number, decimal initialBalance)
        {
            this.number = number;
            this.balance = initialBalance;
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
            Causes(new AmountWithdrawn(withdraw.TransactionId, number, withdraw.Amount));
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
}