using System;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.TestKit.Xunit;
using Lab.AkkaNet.Banking.Actors.EventSourcedExample;
using Xunit;

namespace Lab.AkkaNet.Banking.Tests
{
    public class EventSourcedTests : TestKit
    {

                private static string GetConfigurationString() => $@"
akka {{
    stdout-loglevel = DEBUG
    loglevel = DEBUG
    log-config-on-start = on        
    actor {{                
        debug {{  
                receive = on 
                autoreceive = on
                lifecycle = on
                event-stream = on
                unhandled = on
        }}
    }}
}}";

        public EventSourcedTests()  
            : base(GetConfigurationString())
        {
            
        }

        [Fact]
        public async void SimpleTransfer()
        {
            var eventProbe = CreateTestProbe("events");
            Sys.EventStream.Subscribe(eventProbe, typeof(TransferSucceeded));

            var bank = ActorOfAsTestActorRef<Bank>(Bank.Create("Sparkasse")); // Sys.ActorOf(Bank.Create("Sparkasse"), "Bank-Sparkasse");

            bank.Tell(new Open(1, 100)); // bob
            bank.Tell(new Open(2, 100)); // sam
            bank.Tell(new Transfer(1, 2, 50));

            eventProbe.ExpectMsg<TransferSucceeded>();

            var bobBalance = await bank.Ask<double>(new QueryAccountBalance(1));
            var samBalance = await bank.Ask<double>(new QueryAccountBalance(2));

            Assert.Equal(50, bobBalance);
            Assert.Equal(150, samBalance);
        }

        [Fact]
        public async void InsufficientBalance()
        {
            var eventProbe = CreateTestProbe("events");
            Sys.EventStream.Subscribe(eventProbe, typeof(TransferCanceled));

            var bank = Sys.ActorOf(Bank.Create("Sparkasse"), "Bank-Sparkasse");

            bank.Tell(new Open(1, 100)); // bob
            bank.Tell(new Open(2, 100)); // sam
            
            bank.Tell(new Transfer(1, 2, 500));
            eventProbe.ExpectMsg<TransferCanceled>();

            var bobBalance = await bank.Ask<double>(new QueryAccountBalance(1));
            var samBalance = await bank.Ask<double>(new QueryAccountBalance(2));

            Assert.Equal(100, bobBalance);
            Assert.Equal(100, samBalance);
        }


        [Fact]
        public async void TheMillionaresGame()
        {
            var transactionCount = 10000;
            var bank = Sys.ActorOf(Bank.Create("Sparkasse"), "Bank-Sparkasse");

            bank.Tell(new Open(1, 1000000)); // bob
            bank.Tell(new Open(2, 1000000)); // sam

            var bobToSam = Task.Run(() =>
            {
                for (int i = 0; i < transactionCount; i++)
                {
                    bank.Tell(new Transfer(1, 2, 1));
                }
            });

            var samToBob = Task.Run(() =>
            {
                for (int i = 0; i < transactionCount; i++)
                {
                   bank.Tell(new Transfer(2, 1, 1));
                }
            });

            Task.WaitAll(bobToSam, samToBob);

            do
            {
                var result = await bank.Ask<AuditResult>(new Audit());
                if (result.SucceededTransfers + result.CanceledTransfers == transactionCount * 2)
                    break;
            } while (true) ;

            var bobBalance = await bank.Ask<double>(new QueryAccountBalance(1));
            var samBalance = await bank.Ask<double>(new QueryAccountBalance(2));

            Assert.Equal(1000000, samBalance);
            Assert.Equal(1000000, bobBalance);
        }
    }
}
