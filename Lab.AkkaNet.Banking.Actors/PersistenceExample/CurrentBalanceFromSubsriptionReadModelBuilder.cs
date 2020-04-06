using System;
using System.Threading.Tasks;
using Akka;
using Akka.Actor;
using Akka.Persistence.Query;
using Akka.Persistence.Query.Sql;
using Akka.Streams;
using Akka.Streams.Dsl;
using Akka.Streams.Stage;

namespace Lab.AkkaNet.Banking.Actors.PersistenceExample
{
    public class CurrentBalanceFromSubsriptionReadModelBuilder : ReceiveActor
    {

        public static Props Create(SqlReadJournal journal, AccountBalanceDatabase database) => Props.Create(() => new CurrentBalanceFromSubsriptionReadModelBuilder(journal, database));
        
        private SqlReadJournal journal;
        private AccountBalanceDatabase database;
        private NotUsed stream;

        public CurrentBalanceFromSubsriptionReadModelBuilder(SqlReadJournal journal, AccountBalanceDatabase database)
        {
            this.journal = journal;
            this.database = database;

            Receive<EventEnvelope>(OnEvent);
        }        

        protected override async void PreStart()
        {
            journal.EventsByTag("Test")
                .To(Sink.ActorRef<EventEnvelope>(Context.Self, null))
                .Run(ActorMaterializer.Create(Context.System));

            // journal.EventsByTag(string.Empty)
            //     .RunWith(Sink.ActorRef<EventEnvelope>(Context.Self, new object()), ActorMaterializer.Create(Context.System));
        }

        protected override void PostStop()
        {

        }

        private void OnEvent(EventEnvelope arg)
        {
            ((dynamic)this).Handle((dynamic)arg.Event);
        }

       public Task Handle(AccountOpened accountOpened)
        {
            database.Insert(accountOpened.Number, accountOpened.InitialBalance);
            return Task.CompletedTask;
        }

        public Task Handle(object e)
        {
            return Task.CompletedTask;
        }

        public Task Handle(AmountDeposited amountDeposited)
        {
            var currentBalance = database.Select(amountDeposited.Number);
            database.Update(amountDeposited.Number, currentBalance + amountDeposited.Amount);
            return Task.CompletedTask;
        }

        public Task Handle(AmountWithdrawn amountWithdrawn)
        {
            var currentBalance = database.Select(amountWithdrawn.Number);
            database.Update(amountWithdrawn.Number, currentBalance - amountWithdrawn.Amount);
            return Task.CompletedTask;
        }
    }
}