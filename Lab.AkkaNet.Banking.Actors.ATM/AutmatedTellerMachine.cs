using System;
using Akka.Actor;
using Lab.AkkaNet.Banking.Actors.Messages;

namespace Lab.AkkaNet.Banking.Actors.ATM
{
    public class AutomatedTellerMachine : ReceiveActor
    {
        public AutomatedTellerMachine(){

            Receive<Guid>(s => {
                Context.ActorSelection("akka.tcp://Banking@localhost:8199/user/Sparkasse")
                .Tell(new Connect() {
                    ClientId = s
                });
            });
            Receive<Connected>(msg => {
                Console.WriteLine("{0} connected to {1}", msg.ClientId, msg.To);
            });
        }    
    }
}