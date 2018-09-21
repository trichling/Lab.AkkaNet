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
        public void TheFundraiser()
        {
            var bankingSystem = ActorSystem.Create("bankingSystem");
            var bobsAccount = bankingSystem.ActorOf(Account.Create(1, 0));

            var tasks = new List<Task>();
            for (int i = 0; i < 100000; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    bobsAccount.Tell(new Deposit(1, 1));
                }));
            }

            Task.WaitAll(tasks.ToArray());

            var balance = bobsAccount.Ask<decimal>(new QueryBalance(1)).Result;
        }

        [Fact]
        public void TheMillionaresGame()
        {
            var bankingSystem = ActorSystem.Create("bankingSystem");
            var bank = bankingSystem.ActorOf(Bank.Create("Sparkasse"), "Bank-Sparkasse");

            bank.Tell(new Open(1, 1000000)); // bob
            bank.Tell(new Open(2, 1000000)); // sam

            var bobToSam = Task.Run(() =>
            {
                for (int i = 0; i < 1000000; i++)
                {
                    bank.Tell(new Transfer(1, 2, 1));
                }
            });

            var samToBob = Task.Run(() =>
            {
                for (int i = 0; i < 1000000; i++)
                {
                    bank.Tell(new Transfer(2, 1, 1));
                }
            });

            Task.WaitAll(bobToSam, samToBob);

            var bobBalance = bank.Ask<decimal>(new QueryAccountBalance(1)).Result;
            var samaBalance = bank.Ask<decimal>(new QueryAccountBalance(2)).Result;
        }

        [Fact]
        public void TheLuckyLooserWithTheSadWinner()
        {
            var bankingSystem = ActorSystem.Create("bankingSystem");
            var bank = bankingSystem.ActorOf(Bank.Create("Sparkasse"), "Bank-Sparkasse");

            bank.Tell(new Open(1, 0)); // bob
            bank.Tell(new Open(2, 0)); // sam

            var tasks = new List<Task>();
            for (int i = 0; i < 100000; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    bank.Tell(new Transfer(1, 2, 1));
                }));
            }

            Task.WaitAll(tasks.ToArray());

            var luckyLooserBalance = bank.Ask<decimal>(new QueryAccountBalance(1)).Result;
            var sadWinnerBalance = bank.Ask<decimal>(new QueryAccountBalance(2)).Result;
        }


    }
}