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
            var selection = Context.ActorSelection($"/user/{connect.BankName}");
            var requestedBank = selection.Ask<ActorIdentity>(new Identify(null)).Result;

            Causes(new AtmConnected() {
                AtmId = connect.AtmId,
                ToBank = requestedBank.Subject
            });
        }

        public void Handle(AtmConnected connected)
        {
            
        }

    }
}