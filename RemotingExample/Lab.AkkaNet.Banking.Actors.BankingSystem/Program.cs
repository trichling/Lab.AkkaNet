using System;
using Akka.Actor;

namespace Lab.AkkaNet.Banking.Actors.BankingSystem
{
    class Program
    {
        static void Main(string[] args)
        {
            var bankName = "Sparkasse";
            var port = 8199;

            if (args.Length == 2)
            {
                bankName = args[0];
                port = int.Parse(args[1]);
            }

            Console.WriteLine($"Starting {bankName}");

            var system = ActorSystem.Create($"Bank-{bankName}", GetConfigurationString(port));

            var connector = system.ActorOf<BankConnector>("BankConnector");
            var sparkasse = system.ActorOf(Bank.Create(bankName), bankName);

            Console.ReadLine();
        }

        private static string GetConfigurationString(int port) => $@"
akka {{
    stdout-loglevel = DEBUG
    loglevel = DEBUG
    log-config-on-start = on        
    actor {{       
        provider = remote        
        debug {{  
                receive = on 
                autoreceive = on
                lifecycle = on
                event-stream = on
                unhandled = on
        }}
    }}
    remote {{
            dot-netty.tcp {{
                port = {port} 
                hostname = localhost
            }}
        }}
}}";
    }
}
