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
            var bankName = "Sparkasse";
            var port = 8199;

            if (args.Length == 2)
            {
                bankName = args[0];
                port = int.Parse(args[1]);
            }
            var system = ActorSystem.Create("ATM", GetConfigurationString());
            var atm = system.ActorOf(Props.Create(() => new AutomatedTellerMachine(bankName, port)));
            

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
