﻿using System;
using System.Collections.Generic;
using System.Text;
using Akka.Actor;
using Lab.AkkaNet.Banking.Actors.ActorBase;
using Lab.AkkaNet.Banking.Actors.Serialization;

namespace Lab.AkkaNet.Banking.Actors.PersistenceExample
{

    public class Bank : EventSourcedUntypedPresistentActor
    {
        public static Props Create(string name) => Props.Create(() => new Bank(name));

        private string name;

        public Bank(string name)
        {
            this.name = name;
        }

        public override string PersistenceId => $"Bank-{name}";

        protected override ISnapshot GetSnapshot(){
            return new BankSnapshot
            {
                Name = this.name
            };
        }

        protected override void RestoreFromSnapshop(ISnapshot snapshot)
        {
            if (snapshot is BankSnapshot bankSnapshot)
            {
                this.name = bankSnapshot.Name;
            }
        }

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

    public class BankSnapshot : ISnapshot
    {

        public string Name { get; set; }  

    }

    public class QueryAccountBalance
    {

        public QueryAccountBalance(int number)
        {
            Number = number;
        }

        public int Number { get; }

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

    [Tags("Account")]
    public class AccountOpened : IEvent
    {
        
        public AccountOpened(int number, decimal initialBalance)
        {
            Number = number;
            InitialBalance = initialBalance;
        }

        public int Number { get; }
        public decimal InitialBalance { get; }

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

  
    public class MoneyTransfered : IEvent
    { 

        public MoneyTransfered(Guid transactionId, int sourceAccountNumber, int targetAccountNumber, decimal amount)
        {
            TransactionId = transactionId;
            SourceAccountNumber = sourceAccountNumber;
            TargetAccountNumber = targetAccountNumber;
            Amount = amount;
        }

        public Guid TransactionId { get; }
        public int SourceAccountNumber { get; }
        public int TargetAccountNumber { get; }
        public decimal Amount { get; }
    }


}
