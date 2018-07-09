using Akka.Actor;

namespace Lab.AkkaNet.Banking.Actors.ActorBase
{
    public abstract class EventSourcedUntypedActor : UntypedActor
    {
       
        protected override void OnReceive(object message)
        {
            ((dynamic)this).Handle((dynamic)message);
        }

        public void Handle(object message)
        { }

        protected void Causes(object @event)
        {
            DispatchToApply(@event);
            Context.System.EventStream.Publish(@event);
            if (Sender != null)
                Sender.Tell(@event);
        }

        private void DispatchToApply(object @event)
        {
            ((dynamic)this).Apply((dynamic)@event);
        }

        // Catch All
        public void Apply(object e)
        {
        }
    }
}
