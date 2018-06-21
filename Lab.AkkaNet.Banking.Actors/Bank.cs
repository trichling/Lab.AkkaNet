using System;
using System.Collections.Generic;
using System.Text;
using Akka.Actor;

namespace Lab.AkkaNet.Banking.Actors
{
    public class Bank : UntypedActor
    {
        public static Props Create(string name) => Props.Create(() => new Bank(name));

        private string name;

        public Bank(string name)
        {
            this.name = name;
        }

        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case Open open:
                    var newAccount = Context.ActorOf(Account.Create(open.Number, open.InitialBalance), $"Account-{open.Number}");
                    Sender.Tell(newAccount);
                    break;

                case Transfer transfer:
                    var sourceAccount = Context.Child($"Account-{transfer.SourceAccountNumber}");
                    var targetAccount = Context.Child($"Account-{transfer.TargetAccountNumber}");

                    sourceAccount.Tell(new Withdraw(transfer.SourceAccountNumber, transfer.Amount));
                    targetAccount.Tell(new Deposit(transfer.TargetAccountNumber, transfer.Amount));
                    break;

                case QueryAccountBalance queryAccountBalance:
                    var account = Context.Child($"Account-{queryAccountBalance.Number}");
                    account.Forward(new QueryBalance(queryAccountBalance.Number));
                    //account.Tell(new QueryBalance(queryAccountBalance.Number), Self);
                    //account.Tell(new QueryBalance(queryAccountBalance.Number), Sender);

                    //var amount = account.Ask<double>(new QueryBalance(queryAccountBalance.Number)).Result;
                    break;
            }
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

        public Transfer(int sourceAccountNumber, int targetAccountNumber, double amount)
        {
            SourceAccountNumber = sourceAccountNumber;
            TargetAccountNumber = targetAccountNumber;
            Amount = amount;
        }
        public int SourceAccountNumber { get; }
        public int TargetAccountNumber { get; }
        public double Amount { get; }
    }

    public class Open
    {
        
        public Open(int number, double initialBalance)
        {
            Number = number;
            InitialBalance = initialBalance;
        }

        public int Number { get; }
        public double InitialBalance { get; }

    }
}
