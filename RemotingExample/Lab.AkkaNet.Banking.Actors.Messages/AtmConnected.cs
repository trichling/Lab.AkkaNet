using System;
using Akka.Actor;
using Lab.AkkaNet.Banking.Actors.Messages.ActorBase;

namespace Lab.AkkaNet.Banking.Actors.Messages
{
    public class AtmConnected : IEvent
    {

        public Guid AtmId { get; set; }

        public IActorRef ToBank { get; set; }
    }

}