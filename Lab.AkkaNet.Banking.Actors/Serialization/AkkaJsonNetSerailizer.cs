using System;
using System.Text;
using Akka.Actor;
using Akka.Serialization;
using Newtonsoft.Json;

namespace Lab.AkkaNet.Banking.Actors.Serialization
{
    public class AkkaJsonNetSerailizer : SerializerWithStringManifest
    {
        public AkkaJsonNetSerailizer(ExtendedActorSystem system) : base(system)
        {
            
        }


        public override byte[] ToBinary(object obj)
        {
            var serialized = JsonConvert.SerializeObject(obj);
            var bytes = Encoding.UTF8.GetBytes(serialized);
            return bytes;
        }

        public override object FromBinary(byte[] bytes, string manifest)
        {
            var type = Type.GetType(
                $"Lab.AkkaNet.Banking.Actors.PersistenceExample.{manifest}, Lab.AkkaNet.Banking.Actors");
            var json = Encoding.UTF8.GetString(bytes);
            var obj = JsonConvert.DeserializeObject(json, type);
            return obj;
        }

        public override string Manifest(object o)
        {
            return o.GetType().Name;
        }

    }
}
