using Akka.Actor;
using Akka.Persistence;

namespace Lab.AkkaNet.Banking.Actors.ActorBase
{
    public abstract class EventSourcedUntypedPresistentActor : UntypedPersistentActor
    {
        protected override void OnCommand(object command)
        {
            ((dynamic)this).Handle((dynamic)command);
        }

        // Catch all
        public void Handle(object e)
        {
        }

        public void Causes(object @event)
        {
            Persist(@event, DispatchToApply);
            Context.System.EventStream.Publish(@event);
            if (Sender != null)
                Sender.Tell(@event);
        }

        protected override void OnRecover(object @event)
        {
            DispatchToApply(@event);
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
