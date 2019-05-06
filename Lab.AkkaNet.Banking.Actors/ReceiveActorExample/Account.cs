using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;

namespace Lab.AkkaNet.Banking.Actors.ReceiveActorExample
{
    public class Account : ReceiveActor
    {

        public static Props Create(int number, decimal initialBalance) => Props.Create(() => new Account(number, initialBalance));

        private int number;
        private decimal balance;

        public Account(int number, decimal initialBalance)
        {
            this.number = number;
            this.balance = initialBalance;

            ReceiveAsync<Deposit>(Handle);
            ReceiveAsync<Withdraw>(Handle);
            ReceiveAsync<QueryAccountBalance>(Handle);
        }

        public Task Handle(Deposit message)
        {
            balance += message.Amount;
            return Task.CompletedTask;
        }

        public Task Handle(Withdraw message)
        {
            balance -= message.Amount;
            return Task.CompletedTask;
        }

        public Task Handle(QueryAccountBalance message)
        {
            Sender.Tell(balance);
            return Task.CompletedTask;
        }
      
    }

   

    public class QueryBalance
    {

        public QueryBalance(int number)
        {
            Number = number;
        }
        public int Number { get; set; }

    }

    public class Deposit
    {
        public Deposit(int number, decimal amount)
        {
            Number = number;
            Amount = amount;
        }
        public int Number { get; set; }
        public decimal Amount { get; set; }

    }

    public class Withdraw
    {

        public Withdraw(int number, decimal amount)
        {
            Number = number;
            Amount = amount;
        }

        public int Number { get; set; }
        public decimal Amount { get; set; }

    }
}
