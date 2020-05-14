using System;
using Akka.Actor;
using Lab.AkkaNet.Banking.Actors.ATM.Messges;
using Lab.AkkaNet.Banking.Actors.Messages;

namespace Lab.AkkaNet.Banking.Actors.ATM
{
    class Program
    {
        static void Main(string[] args)
        {
            var system = ActorSystem.Create("ATM", GetConfigurationString());

            // Can not receive Connected message
            // var sparkasse = system.ActorSelection("akka.tcp://Banking@localhost:8199/user/Sparkasse");
            // sparkasse.Tell(new Connect() {
            //     ClientId = Guid.NewGuid()
            // });

            // better: use an actor for it
            var atm = system.ActorOf(Props.Create(() => new AutomatedTellerMachine()));
            atm.Tell(new ConnectToBank() {
                AtmId = Guid.NewGuid(),
                BankName = "Sparkasse"
            });

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
