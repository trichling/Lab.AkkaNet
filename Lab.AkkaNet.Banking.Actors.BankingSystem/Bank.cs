using System;
using Akka.Actor;
using Lab.AkkaNet.Banking.Actors.Messages;
using Lab.AkkaNet.Banking.Actors.Messages.ActorBase;

namespace Lab.AkkaNet.Banking.Actors.BankingSystem
{
   public class Bank : EventSourcedUntypedActor
    {
     public static Props Create(string name) => Props.Create(() => new Bank(name));

        private string name;

        public Bank(string name)
        {
            this.name = name;
        }

        public void Handle(Connect connect)
        {
            Causes(new Connected() {
                ClientId = connect.ClientId,
                To = this.name
            });
        }

        public void Handle(Connected connected)
        {}

        public void Handle(QueryAccountBalance queryAccountBalance)
        {
            var account = Context.Child($"Account-{queryAccountBalance.Number}");
            account.Forward(new QueryBalance(queryAccountBalance.Number));
        }

        public void Handle(Open open)
        {
            if (AccountDoesNotExist(open.Number))
            {
                Causes(new AccountOpened(open.Number, open.InitialBalance));
                return;
            }

            // error - account alredy exists
        }

        private bool AccountDoesNotExist(int number)
        {
            var account = Context.Child($"Account-{number}");
            return account == ActorRefs.Nobody;
        }

        public void Apply(AccountOpened accountOpened)
        {
            Context.ActorOf(Account.Create(accountOpened.Number, accountOpened.InitialBalance), $"Account-{accountOpened.Number}");
        }

        public void Handle(Transfer transfer)
        {
            var sourceAccount = Context.Child($"Account-{transfer.SourceAccountNumber}");
            var targetAccount = Context.Child($"Account-{transfer.TargetAccountNumber}");

            var transactionId = Guid.NewGuid();
            sourceAccount.Tell(new Withdraw(transactionId, transfer.SourceAccountNumber, transfer.Amount));
            targetAccount.Tell(new Deposit(transactionId, transfer.TargetAccountNumber, transfer.Amount));
            Causes(new MoneyTransfered(transactionId, transfer.SourceAccountNumber, transfer.TargetAccountNumber, transfer.Amount));
        }
       
        public void Apply(MoneyTransfered successfulTransfer)
        {
           
        }


    }
}