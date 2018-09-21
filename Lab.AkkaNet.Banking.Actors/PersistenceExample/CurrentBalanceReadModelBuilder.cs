using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using Lab.AkkaNet.Banking.Actors.ActorBase;

namespace Lab.AkkaNet.Banking.Actors.PersistenceExample
{

    public class AccountBalanceDatabase
    {

        private Dictionary<int, decimal> _accountBalances = new Dictionary<int, decimal>();

        public decimal Select(int accountNumber)
        {
            return _accountBalances[accountNumber];
        }

        public void Insert(int accountNumber, decimal balance)
        {
            _accountBalances.Add(accountNumber, balance);
        }

        public void Update(int accountNumber, decimal balance)
        {
            _accountBalances[accountNumber] = balance;
        }

    }

    public class CurrentBalanceReadModelBuilder : ReceiveActor
    {

        public static Props Create(AccountBalanceDatabase database) => Props.Create(() => new CurrentBalanceReadModelBuilder(database));

        private readonly AccountBalanceDatabase database;

        public CurrentBalanceReadModelBuilder(AccountBalanceDatabase database)
        {
            this.database = database;

            ReceiveAsync<AccountOpened>(AccountOpened);
            ReceiveAsync<AmountDeposited>(AmountDeposited);
            ReceiveAsync<AmountWithdrawn>(AmountWithdrawn);
        }

        protected override async void PreStart()
        {
            Context.System.EventStream.Subscribe(Self, typeof(AccountOpened));
            Context.System.EventStream.Subscribe(Self, typeof(AmountDeposited));
            Context.System.EventStream.Subscribe(Self, typeof(AmountWithdrawn));
        }

        public Task AccountOpened(AccountOpened accountOpened)
        {
            database.Insert(accountOpened.Number, accountOpened.InitialBalance);
            return Task.CompletedTask;
        }

        public Task AmountDeposited(AmountDeposited amountDeposited)
        {
            var currentBalance = database.Select(amountDeposited.Number);
            database.Update(amountDeposited.Number, currentBalance + amountDeposited.Amount);
            return Task.CompletedTask;
        }

        public Task AmountWithdrawn(AmountWithdrawn amountWithdrawn)
        {
            var currentBalance = database.Select(amountWithdrawn.Number);
            database.Update(amountWithdrawn.Number, currentBalance - amountWithdrawn.Amount);
            return Task.CompletedTask;
        }

    }

}