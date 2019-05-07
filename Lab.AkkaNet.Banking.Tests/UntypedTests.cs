using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.TestKit.Xunit;
using Lab.AkkaNet.Banking.Actors.UntypedExample;
using Xunit;

namespace Lab.AkkaNet.Banking.Tests
{
    public class UntypedTests : TestKit
    {

        [Fact]
        public async Task TheFundraiser()
        {
            var bankingSystem = ActorSystem.Create("bankingSystem");
            var teslasAccount = bankingSystem.ActorOf(Account.Create(1, 0));

            var tasks = new List<Task>();
            for (int i = 0; i < 100000; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    teslasAccount.Tell(new Deposit(1, 1));
                }));
            }

            Task.WaitAll(tasks.ToArray());

            var balance = await teslasAccount.Ask<decimal>(new QueryBalance(1));
        }

        [Fact]
        public async Task TheMillionaresGame()
        {
            var bankingSystem = ActorSystem.Create("bankingSystem");
            var bank = bankingSystem.ActorOf(Bank.Create("Sparkasse"), "Bank-Sparkasse");

            bank.Tell(new Open(1, 1000000)); 
            bank.Tell(new Open(2, 1000000)); 

            var thomasToAlva = Task.Run(() =>
            {
                for (int i = 0; i < 1000000; i++)
                {
                    bank.Tell(new Transfer(1, 2, 1));
                }
            });

            var alvaToThomas = Task.Run(() =>
            {
                for (int i = 0; i < 1000000; i++)
                {
                    bank.Tell(new Transfer(2, 1, 1));
                }
            });

            Task.WaitAll(thomasToAlva, alvaToThomas);

            var thomasBalance = await bank.Ask<decimal>(new QueryAccountBalance(1));
            var alvaBalance = await bank.Ask<decimal>(new QueryAccountBalance(2));
        }

        [Fact]
        public async Task TheLuckyLooserWithTheSadWinner()
        {
            var bankingSystem = ActorSystem.Create("bankingSystem");
            var bank = bankingSystem.ActorOf(Bank.Create("Sparkasse"), "Bank-Sparkasse");
            var tellProbe = CreateTestProbe();

            bank.Tell(new Open(1, 0), tellProbe); // bob
            var account1 = tellProbe.ExpectMsg<IActorRef>();

            bank.Tell(new Open(2, 0), tellProbe); // sam
            var account2 = tellProbe.ExpectMsg<IActorRef>();


            var tasks = new List<Task>();
            for (int i = 0; i < 100000; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    bank.Tell(new Transfer(1, 2, 1));
                }));
            }

            Task.WaitAll(tasks.ToArray());

            var luckyLooserBalance = await bank.Ask<decimal>(new QueryAccountBalance(1));
            var sadWinnerBalance = await bank.Ask<decimal>(new QueryAccountBalance(2));

            var account1Balance = await account1.Ask<decimal>(new QueryBalance(1));
            var account2Balance = await account2.Ask<decimal>(new QueryBalance(2));

        }


    }
}