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
            var system = ActorSystem.Create("Banking", GetConfigurationString());
            Console.ReadLine();
        }

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
            ""Lab.AkkaNet.Banking.Actors.PersistenceExample.ISnapshot, Lab.AkkaNet.Banking.Actors"" = custom-json
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

            event-adapters {{
                 tagging = ""Lab.AkkaNet.Banking.Actors.Serialization.TaggingEventAdapter, Lab.AkkaNet.Banking.Actors""
            }}

            event-adapter-bindings {{
                ""Lab.AkkaNet.Banking.Actors.PersistenceExample.IEvent, Lab.AkkaNet.Banking.Actors"" = tagging
            }}
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
      
    }
}
