using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Configuration;

namespace Lab.AkkaNet.Banking.Actors
{
    class Program
    {
        static void Main(string[] args)
        {
            var system = ActorSystem.Create("", ConfigurationFactory.ParseString(""));
            Console.ReadLine();
        }
      
    }
}
