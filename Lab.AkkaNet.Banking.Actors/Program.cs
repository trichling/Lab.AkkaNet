using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Akka.Actor;

namespace Lab.AkkaNet.Banking.Actors
{
    class Program
    {
        static void Main(string[] args)
        {
            // UntypedExample.Examples.TheFundraiser();
            // UntypedExample.Examples.TheMillionaresGame();
            // UntypedExample.Examples.TheLuckyLooserWithTheSadWinner();

            EventSourcedExample.Examples.SimpleTransfer();

            Console.ReadLine();
        }
      
    }
}
