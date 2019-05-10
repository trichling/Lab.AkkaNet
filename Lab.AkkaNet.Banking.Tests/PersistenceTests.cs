using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.TestKit.Xunit;
using Lab.AkkaNet.Banking.Actors.PersistenceExample;
using Xunit;
using Dapper;
using Akka.Persistence.Query;
using Akka.Persistence.Query.Sql;
using Akka.Streams;
using System.Collections.Generic;
using System.Linq;
using Akka.Streams.Dsl;
using System.Collections.Immutable;

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
            //""Lab.AkkaNet.Banking.Actors.PersistenceExample.IEvent, Lab.AkkaNet.Banking.Actors"" = custom-json
            //""Lab.AkkaNet.Banking.Actors.PersistenceExample.ISnapshot, Lab.AkkaNet.Banking.Actors"" = custom-json
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
        public async void TheMillionaresGame()
        {
            var connection = new SqlConnection(DbConnectionString);
            connection.Execute("TRUNCATE TABLE Banking_Journal");
            connection.Execute("TRUNCATE TABLE Banking_Snapshot");

            var transactionCount = 100;
            var bank = Sys.ActorOf(Bank.Create("Sparkasse"), "Bank-Sparkasse");

            bank.Tell(new Open(1, 1000000)); 
            bank.Tell(new Open(2, 1000000)); 

            var thomasToAlva = Task.Run(() =>
            {
                for (int i = 0; i < transactionCount; i++)
                {
                    bank.Tell(new Transfer(1, 2, 1));
                }
            });

            var alvaToThomas = Task.Run(() =>
            {
                for (int i = 0; i < transactionCount; i++)
                {
                    bank.Tell(new Transfer(2, 1, 1));
                }
            });

            Task.WaitAll(thomasToAlva, alvaToThomas);

            Thread.Sleep(1000);

            var bobBalance = await bank.Ask<decimal>(new QueryAccountBalance(1));
            var samBalance = await bank.Ask<decimal>(new QueryAccountBalance(2));

            Assert.Equal(1000000, samBalance);
            Assert.Equal(1000000, bobBalance);
        }

        [Fact]
        public async void CurrentBalanceReadModel()
        {
            var connection = new SqlConnection(DbConnectionString);
            connection.Execute("TRUNCATE TABLE Banking_Journal");
            connection.Execute("TRUNCATE TABLE Banking_Snapshot");

            var database = new AccountBalanceDatabase();
            Sys.ActorOf(CurrentBalanceReadModelBuilder.Create(database));

            var bank = ActorOfAsTestActorRef<Bank>(Bank.Create("Sparkasse")); // Sys.ActorOf(Bank.Create("Sparkasse"), "Bank-Sparkasse");

            bank.Tell(new Open(1, 100)); // bob
            bank.Tell(new Open(2, 100)); // sam
            bank.Tell(new Transfer(1, 2, 50));

            Thread.Sleep(1000);

            Assert.Equal(50M, database.Select(1));
            Assert.Equal(150M, database.Select(2));
        }

        [Fact]
        public async void SimpleTransfer()
        {
            var connection = new SqlConnection(DbConnectionString);
            connection.Execute("TRUNCATE TABLE Banking_Journal");
            connection.Execute("TRUNCATE TABLE Banking_Snapshot");

            var eventProbe = CreateTestProbe("events");
            Sys.EventStream.Subscribe(eventProbe, typeof(MoneyTransfered));

            var bank = ActorOfAsTestActorRef<Bank>(Bank.Create("Sparkasse")); // Sys.ActorOf(Bank.Create("Sparkasse"), "Bank-Sparkasse");

            bank.Tell(new Open(1, 100)); // bob
            bank.Tell(new Open(2, 100)); // sam
            bank.Tell(new Transfer(1, 2, 50));

            eventProbe.ExpectMsg<MoneyTransfered>(TimeSpan.FromMinutes(2));

            var bobBalance = await bank.Ask<decimal>(new QueryAccountBalance(1));
            var samBalance = await bank.Ask<decimal>(new QueryAccountBalance(2));

            Assert.Equal(50, bobBalance);
            Assert.Equal(150, samBalance);
        }

        [Fact]
        public async void QueryBalance()
        {
            var bank = ActorOfAsTestActorRef<Bank>(Bank.Create("Sparkasse")); // Sys.ActorOf(Bank.Create("Sparkasse"), "Bank-Sparkasse");
            var bobBalance = await bank.Ask<decimal>(new QueryAccountBalance(1));
            var samBalance = await bank.Ask<decimal>(new QueryAccountBalance(2));
        }

        [Fact]
        public async void CanGetAllAccountIds()
        {
            var connection = new SqlConnection(DbConnectionString);
            connection.Execute("TRUNCATE TABLE Banking_Journal");
            connection.Execute("TRUNCATE TABLE Banking_Snapshot");

            var bank = ActorOfAsTestActorRef<Bank>(Bank.Create("Sparkasse")); // Sys.ActorOf(Bank.Create("Sparkasse"), "Bank-Sparkasse");

            bank.Tell(new Open(1, 100)); // bob
            bank.Tell(new Open(2, 100)); // sam
            bank.Tell(new Transfer(1, 2, 50));

            Thread.Sleep(5000);

            var readJournal = Sys.ReadJournalFor<SqlReadJournal>(SqlReadJournal.Identifier);
            var materializer = ActorMaterializer.Create(Sys);

            var allPersistenceIds = new List<string>();
            var allPersistenceIdsQuery = readJournal.CurrentPersistenceIds();

            await allPersistenceIdsQuery.RunForeach(id =>
            {
                allPersistenceIds.Add(id);
            }, materializer);

            var allAccounts = allPersistenceIds.Where(id => id.StartsWith($"Account-"));

            Assert.Equal(2, allAccounts.Count());
        }

        [Fact]
        public async void CanGetAllAccount1Events()
        {
            var connection = new SqlConnection(DbConnectionString);
            connection.Execute("TRUNCATE TABLE Banking_Journal");
            connection.Execute("TRUNCATE TABLE Banking_Snapshot");

            var bank = ActorOfAsTestActorRef<Bank>(Bank.Create("Sparkasse")); // Sys.ActorOf(Bank.Create("Sparkasse"), "Bank-Sparkasse");

            bank.Tell(new Open(1, 100)); // bob
            bank.Tell(new Open(2, 100)); // sam
            bank.Tell(new Transfer(1, 2, 50));

            Thread.Sleep(5000);

            var readJournal = Sys.ReadJournalFor<SqlReadJournal>(SqlReadJournal.Identifier);
            var materializer = ActorMaterializer.Create(Sys);

            var historyOfAccount = new List<object>();
            var historyOfAccountQuery = readJournal.CurrentEventsByPersistenceId("Account-1", 0L, long.MaxValue);
            var result = await historyOfAccountQuery
                .Select(c => c.Event) //.RunForeach(e => historyOfStock.Add(e), _actorMaterializer);
                .RunAggregate(
                    ImmutableHashSet<object>.Empty,
                    (acc, c) => acc.Add(c),
                    materializer);

            Assert.Equal(1, result.ToList().Count);
            Assert.True(result.ToList()[0] is AmountWithdrawn);
        }

        

       
    }
}
