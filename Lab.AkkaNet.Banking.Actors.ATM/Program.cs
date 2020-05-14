using System;
using Akka.Actor;

namespace Lab.AkkaNet.Banking.Actors.ATM
{
    class Program
    {
        static void Main(string[] args)
        {
            var system = ActorSystem.Create("ATM", GetConfigurationString());

            var atm = system.ActorOf(Props.Create(() => new AutomatedTellerMachine()));
            atm.Tell(Guid.NewGuid());

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
                port = 0 # Dynamic Port (Client)
                hostname = localhost
            }}
        }}
}}";
    }
}
