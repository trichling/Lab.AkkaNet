using System;

namespace Lab.AkkaNet.Banking.Actors.Serialization
{
    [AttributeUsage(validOn: AttributeTargets.Class)]
    public class TagsAttribute : Attribute
    {
        
        public TagsAttribute(params string[] tags)
        {
            Tags = tags;
        }

        public string[] Tags { get; }
    }
}