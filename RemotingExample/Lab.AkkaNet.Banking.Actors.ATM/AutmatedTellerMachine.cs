using System;
using Akka.Actor;
using Lab.AkkaNet.Banking.Actors.ATM.Messges;
using Lab.AkkaNet.Banking.Actors.Messages;

namespace Lab.AkkaNet.Banking.Actors.ATM
{
    public class AutomatedTellerMachine : ReceiveActor
    {
        public AutomatedTellerMachine(){

            Receive<ConnectToBank>(s => {
                Context.ActorSelection("akka.tcp://Banking@localhost:8199/user/BankConnector")
                .Tell(new ConnectWithAtm() {
                    AtmId = s.AtmId,
                    BankName = s.BankName
                });
            });
            
            Receive<AtmConnected>(msg => {
                Console.WriteLine("{0} connected to {1}", msg.AtmId, msg.ToBank);
                this.Bank = msg.ToBank;
            });
        }    

        public IActorRef Bank { get; set; }
    }
}