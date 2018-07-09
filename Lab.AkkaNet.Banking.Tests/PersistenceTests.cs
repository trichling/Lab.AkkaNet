using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.TestKit.Xunit;
using Lab.AkkaNet.Banking.Actors.PersistenceExample;
using Xunit;
using Dapper;

namespace Lab.AkkaNet.Banking.Tests
{
    public class PersistenceTests : TestKit
    {

        private static string DbConnectionString = "Server = (local); Database=LabAkkaBanking;Trusted_Connection=True;MultipleActiveResultSets=true";

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
        serializers {{
            custom-json = ""Lab.AkkaNet.Banking.Actors.Serialization.AkkaJsonNetSerailizer, Lab.AkkaNet.Banking.Actors""
        }}
        serialization-bindings {{
            ""Lab.AkkaNet.Banking.Actors.PersistenceExample.IEvent, Lab.AkkaNet.Banking.Actors"" = custom-json
        }}
        serialization-identifiers {{
            ""Lab.AkkaNet.Banking.Actors.Serialization.AkkaJsonNetSerailizer, Lab.AkkaNet.Banking.Actors"" = 42
        }}
    }}
    persistence {{
        journal.plugin = ""akka.persistence.journal.sql-server""
        journal.sql-server {{
            class = ""Akka.Persistence.SqlServer.Journal.SqlServerJournal, Akka.Persistence.SqlServer""
            plugin-dispatcher = ""akka.actor.default-dispatcher""
            auto-initialize = on
            connection-string = ""{DbConnectionString}""
            schema-name = dbo
            table-name = Banking_Journal
            refresh-interval = 1s
        }}
        snapshot-store.plugin =  ""akka.persistence.snapshot-store.sql-server""
        snapshot-store.sql-server {{
            # qualified type name of the SQL Server persistence journal actor
            class = ""Akka.Persistence.SqlServer.Snapshot.SqlServerSnapshotStore, Akka.Persistence.SqlServer""

            # dispatcher used to drive journal actor
            plugin-dispatcher = ""akka.actor.default-dispatcher""
            
            # should corresponding journal table be initialized automatically
            auto-initialize = on

            # connection string used for database access
            connection-string = ""{DbConnectionString}""

            # SQL server schema name to table corresponding with persistent journal
            schema-name = dbo

            # SQL server table corresponding with persistent journal
            table-name = Banking_Snapshot
        }}
    }}

}}";

       

        public PersistenceTests()  
            : base(GetConfigurationString())
        {

        }

        public void Dispose()
        {
            // Wait for journal
            Thread.Sleep(TimeSpan.FromSeconds(10));
        }

        [Fact]
        public async void QueryBalance()
        {
            var bank = ActorOfAsTestActorRef<Bank>(Bank.Create("Sparkasse")); // Sys.ActorOf(Bank.Create("Sparkasse"), "Bank-Sparkasse");
            var bobBalance = await bank.Ask<double>(new QueryAccountBalance(1));
            var samBalance = await bank.Ask<double>(new QueryAccountBalance(2));
        }

        [Fact]
        public async void SimpleTransfer()
        {
            var connection = new SqlConnection(DbConnectionString);
            connection.Execute("TRUNCATE TABLE Banking_Journal");

            var eventProbe = CreateTestProbe("events");
            Sys.EventStream.Subscribe(eventProbe, typeof(TransferSucceeded));

            var bank = ActorOfAsTestActorRef<Bank>(Bank.Create("Sparkasse")); // Sys.ActorOf(Bank.Create("Sparkasse"), "Bank-Sparkasse");

            bank.Tell(new Open(1, 100)); // bob
            bank.Tell(new Open(2, 100)); // sam
            bank.Tell(new Transfer(1, 2, 50));

            eventProbe.ExpectMsg<TransferSucceeded>(TimeSpan.FromMinutes(2));

            var bobBalance = await bank.Ask<double>(new QueryAccountBalance(1));
            var samBalance = await bank.Ask<double>(new QueryAccountBalance(2));

            Assert.Equal(50, bobBalance);
            Assert.Equal(150, samBalance);
        }

        [Fact]
        public async void InsufficientBalance()
        {
            var connection = new SqlConnection(DbConnectionString);
            connection.Execute("TRUNCATE TABLE Banking_Journal");

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
            var connection = new SqlConnection(DbConnectionString);
            connection.Execute("TRUNCATE TABLE Banking_Journal");

            var transactionCount = 1000;
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

            Thread.Sleep(1000);

            var bobBalance = await bank.Ask<double>(new QueryAccountBalance(1));
            var samBalance = await bank.Ask<double>(new QueryAccountBalance(2));

            Assert.Equal(1000000, samBalance);
            Assert.Equal(1000000, bobBalance);
        }
    }
}
