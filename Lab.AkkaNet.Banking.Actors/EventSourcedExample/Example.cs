using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;

namespace Lab.AkkaNet.Banking.Actors.EventSourcedExample
{

    public class Examples
    {

        public static void SimpleTransfer()
        {
            var bankingSystem = ActorSystem.Create("bankingSystem");
            var bank = bankingSystem.ActorOf(Bank.Create("Sparkasse"), "Bank-Sparkasse");

            bank.Tell(new Open(1, 100)); // bob
            bank.Tell(new Open(2, 100)); // sam
            bank.Tell(new Transfer(1, 2, 50));

            Thread.Sleep(1000);

            var bobBalance = bank.Ask<double>(new QueryAccountBalance(1)).Result;
            var samaBalance = bank.Ask<double>(new QueryAccountBalance(2)).Result;
        }

        public static void TheFundraiser()
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

            var balance = bobsAccount.Ask<double>(new QueryBalance(1)).Result;
        }

        public static void TheMillionaresGame()
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

            var bobBalance = bank.Ask<double>(new QueryAccountBalance(1)).Result;
            var samaBalance = bank.Ask<double>(new QueryAccountBalance(2)).Result;
        }

    }

}