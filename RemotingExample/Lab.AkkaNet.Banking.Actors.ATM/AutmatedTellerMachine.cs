using System;
using Akka.Actor;
using Lab.AkkaNet.Banking.Actors.ATM.Messges;
using Lab.AkkaNet.Banking.Actors.Messages;

namespace Lab.AkkaNet.Banking.Actors.ATM
{
    public class AutomatedTellerMachine : ReceiveActor
    {

        private string remoteAddress;
        private IActorRef healthChecker;
        private ICancelable heartbeatTask;
        private IActorRef bank;
        private Guid atmId;


        public AutomatedTellerMachine(Guid atmId, string bank, int port)
        {
            this.atmId = atmId;
            this.remoteAddress = $"akka.tcp://Bank-{bank}@localhost:{port}";

            Receive<ConnectToBank>(s => {
                Context.ActorSelection($"{remoteAddress}/user/BankConnector")
                .Tell(new ConnectWithAtm() {
                    AtmId = atmId,
                    BankName = s.BankName
                });
            });
            
            Receive<AtmConnected>(msg => {
                Console.WriteLine("{0} connected to {1}", msg.AtmId, msg.ToBank);
                this.bank = msg.ToBank;

                var remoteSystem = Address.Parse(remoteAddress);
                healthChecker =
                    Context.System.ActorOf(
                        Props.Create(() => new HealthChecker())
                             .WithDeploy(Deploy.None.WithScope(new RemoteScope(remoteSystem))), 
                        $"HealthChecker"); 

                // Set death watch
                Context.Watch(healthChecker);

                heartbeatTask = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(1), 
                    Context.Self, new SendHeartbeat(), 
                    ActorRefs.NoSender);
            });

            Receive<SendHeartbeat>(h => {
                healthChecker.Tell(new Heartbeat() {
                    SendTimestamp = DateTime.Now.Ticks,
                    AtmId = atmId
                });
            });

            Receive<Heartbeat>(h => {
                Console.WriteLine($"Latency {(DateTime.Now.Ticks - h.SendTimestamp) / TimeSpan.TicksPerMillisecond} ms.");
            });

            Receive<Terminated>(t => {
                Console.WriteLine("Bank died!!");
                Context.Stop(Self);
            });
        }    
        protected override void PostStop()
        {
            heartbeatTask.Cancel();
        }
    }
}