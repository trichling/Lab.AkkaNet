using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;

namespace Lab.AkkaNet.Banking.Actors.ReceiveActorExample
{
    public class Bank : ReceiveActor
    {
        public static Props Create(string name) => Props.Create(() => new Bank(name));

        private string name;

        public Bank(string name)
        {
            this.name = name;

            ReceiveAsync<Open>(Handle);
            ReceiveAsync<Transfer>(Handle);
            ReceiveAsync<QueryAccountBalance>(Handle);
        }

        public Task Handle(Open message)
        {
            var newAccount = Context.ActorOf(Account.Create(message.Number, message.InitialBalance), $"Account-{message.Number}");
            Sender.Tell(newAccount);
            return Task.CompletedTask;
        }

        public Task Handle(Transfer message)
        {
            var sourceAccount = Context.Child($"Account-{message.SourceAccountNumber}");
            var targetAccount = Context.Child($"Account-{message.TargetAccountNumber}");

            sourceAccount.Tell(new Withdraw(message.SourceAccountNumber, message.Amount));
            targetAccount.Tell(new Deposit(message.TargetAccountNumber, message.Amount));
            return Task.CompletedTask;
        }

        public Task Handle(QueryAccountBalance message)
        {
            var account = Context.Child($"Account-{message.Number}");
            account.Forward(new QueryBalance(message.Number));
            //account.Tell(new QueryBalance(queryAccountBalance.Number), Sender);

            return Task.CompletedTask;
        }
    }

    public class QueryAccountBalance
    {

        public QueryAccountBalance(int number)
        {
            Number = number;
        }

        public int Number { get; }

    }

    public class Transfer
    {

        public Transfer(int sourceAccountNumber, int targetAccountNumber, decimal amount)
        {
            SourceAccountNumber = sourceAccountNumber;
            TargetAccountNumber = targetAccountNumber;
            Amount = amount;
        }
        public int SourceAccountNumber { get; }
        public int TargetAccountNumber { get; }
        public decimal Amount { get; }
    }

    public class Open
    {
        
        public Open(int number, decimal initialBalance)
        {
            Number = number;
            InitialBalance = initialBalance;
        }

        public int Number { get; }
        public decimal InitialBalance { get; }

    }
}
