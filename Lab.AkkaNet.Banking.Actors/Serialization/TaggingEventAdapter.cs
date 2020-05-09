using System.Collections.Generic;
using System.Linq;
using Akka.Persistence.Journal;

namespace Lab.AkkaNet.Banking.Actors.Serialization
{
    public class TaggingEventAdapter : IEventAdapter
    {
        public IEventSequence FromJournal(object evt, string manifest)
        {
            return EventSequence.Single(evt);
        }

        public string Manifest(object evt)
        {
            return evt.GetType().Name;
        }

        public object ToJournal(object evt)
        {
            return new Tagged(evt, GetTags(evt) );
        }

        private IEnumerable<string> GetTags(object evt)
        {
            var result = new List<string>();

            var tagsFromAttributes = evt.GetType()
                .GetCustomAttributes(typeof(TagsAttribute), true)
                .OfType<TagsAttribute>()
                .SelectMany(a => a.Tags);

            result.Add(Manifest(evt));
            result.AddRange(tagsFromAttributes);

            return result;
        }
    }
}