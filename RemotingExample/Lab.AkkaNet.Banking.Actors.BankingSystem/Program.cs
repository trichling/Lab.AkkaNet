using System;
using Akka.Actor;

namespace Lab.AkkaNet.Banking.Actors.BankingSystem
{
    class Program
    {
        static void Main(string[] args)
        {
            var system = ActorSystem.Create("Banking", GetConfigurationString());

            var connector = system.ActorOf<BankConnector>("BankConnector");
            var sparkasse = system.ActorOf(Bank.Create("Sparkasse"), "Sparkasse");

            Console.ReadLine();
        }

        private static string GetConfigurationString() => $@"
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
                port = 8199 # Dynamic Port (Client)
                hostname = localhost
            }}
        }}
}}";
    }
}
