using System;
using Akka.Actor;
using Akka.Persistence;

namespace Lab.AkkaNet.Banking.Actors.Messages.ActorBase
{
    public abstract class EventSourcedUntypedPresistentActor : UntypedPersistentActor
    {

        protected int SnapShotInterval = 10;

        protected override void OnPersistFailure(Exception cause, object @event, long sequenceNr)
        {

        }

        protected override void OnRecoveryFailure(Exception reason, object message = null)
        {

        }

        protected override void OnCommand(object command)
        {            
            ((dynamic)this).Handle((dynamic)command);
        }

        // Catch all
        public virtual void Handle(object e)
        {
            Unhandled(e);
        }

        public void Causes(object @event)
        {
            Persist(@event, Publish);
        }

        private void Publish(object @event)
        {
            DispatchToApply(@event);
            Context.System.EventStream.Publish(@event);
            if (Sender != null)
                Sender.Tell(@event);

            if (LastSequenceNr % SnapShotInterval == 0 && LastSequenceNr != 0)
            {
                SaveSnapshot(GetSnapshot());
            }
        }

        protected abstract ISnapshot GetSnapshot();
        protected abstract void RestoreFromSnapshop(ISnapshot snapshot);

        protected override void OnRecover(object @event)
        {
            if (@event is SnapshotOffer offeredSnapshot)
            {
                RestoreFromSnapshop((dynamic)offeredSnapshot.Snapshot);
                return;
            }

            DispatchToApply(@event);
        }

        private void DispatchToApply(object @event)
        {
            ((dynamic)this).Apply((dynamic)@event);
        }

        // Catch All
        public virtual void Apply(object e)
        {
            Unhandled(e);            
        }

    }
}
