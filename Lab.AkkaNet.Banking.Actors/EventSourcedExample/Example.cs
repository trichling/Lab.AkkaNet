using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Configuration;

namespace Lab.AkkaNet.Banking.Actors.EventSourcedExample
{

    public class Examples
    {

        private static string GetConfigurationString() => $@"
akka {{
    stdout-loglevel = DEBUG
    loglevel = DEBUG
    log-config-on-start = on        
    actor {{                
        debug {{  
                receive = off 
                autoreceive = off
                lifecycle = off
                event-stream = off
                unhandled = off
        }}
    }}
}}";

        public static void TheFundraiser()
        {
            var bankingSystem = ActorSystem.Create("bankingSystem");
            var bobsAccount = bankingSystem.ActorOf(Account.Create(1, 0));

            var tasks = new List<Task>();
            for (int i = 0; i < 100000; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    bobsAccount.Tell(new Deposit(Guid.NewGuid(), 1, 1));
                }));
            }

            Task.WaitAll(tasks.ToArray());

            var balance = bobsAccount.Ask<double>(new QueryBalance(1)).Result;
        }

        public static async void TheMillionaresGame()
        {
            var transactionCount = 10000;
            var bankingSystem = ActorSystem.Create("bankingSystem", ConfigurationFactory.ParseString(GetConfigurationString()));
            var bank = bankingSystem.ActorOf(Bank.Create("Sparkasse"), "Bank-Sparkasse");

            bank.Tell(new Open(1, 1000000)); // bob
            bank.Tell(new Open(2, 1000000)); // sam

            var bobToSam = Task.Run(() =>
            {
                for (int i = 0; i < transactionCount; i++)
                {
                    bank.Tell(new Transfer(1, 2, 1));
                }
            });

            var samToBob = Task.Run(() =>
            {
                for (int i = 0; i < transactionCount; i++)
                {
                   bank.Tell(new Transfer(2, 1, 1));
                }
            });

            Task.WaitAll(bobToSam, samToBob);

            while (true)
            {
                var result = await bank.Ask<AuditResult>(new Audit());

                Console.Clear();
                Console.WriteLine($"{result.OpenTransfers} / {result.SucceededTransfers} / {result.CanceledTransfers} ");

                Thread.Sleep(10);
            }

            var bobBalance = await bank.Ask<double>(new QueryAccountBalance(1));
            var samaBalance = await bank.Ask<double>(new QueryAccountBalance(2));
        }

    }

}