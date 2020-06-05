using System;
using Akka.Actor;
using Lab.AkkaNet.Banking.Actors.Messages;
using Lab.AkkaNet.Banking.Actors.Messages.ActorBase;

namespace Lab.AkkaNet.Banking.Actors.BankingSystem
{
    public class BankConnector : EventSourcedUntypedActor
    {
        
        public BankConnector()
        {
            
        }

        public void Handle(ConnectWithAtm connect)
        {
            Console.WriteLine($"ConnectWithAtm: {Sender}");

            var selection = Context.ActorSelection($"/user/{connect.BankName}");
            var requestedBank = selection.Ask<ActorIdentity>(new Identify(null)).Result;

            Causes(new AtmConnected() {
                AtmId = connect.AtmId,
                ToBank = requestedBank.Subject
            });
        }

        public void Apply(AtmConnected connected)
        {
            Context.Watch(Sender);
            Console.WriteLine($"AtmConnected: {Sender}");
        }

        public void Handle(Terminated terminated)
        {
            Console.WriteLine($"ATM died {terminated.ActorRef}");
        }
    }
}