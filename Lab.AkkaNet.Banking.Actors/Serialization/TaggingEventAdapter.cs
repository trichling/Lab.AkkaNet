using Akka.Persistence.Journal;

namespace Lab.AkkaNet.Banking.Actors.Serialization
{
    public class TaggingEventAdapter : IEventAdapter
    {
        public IEventSequence FromJournal(object evt, string manifest)
        {
            return EventSequence.Single(evt); // identity
        }

        public string Manifest(object evt)
        {
            return evt.GetType().Name;
        }

        public object ToJournal(object evt)
        {
            return new Tagged(evt, new [] { "Test" });
        }
    }
}