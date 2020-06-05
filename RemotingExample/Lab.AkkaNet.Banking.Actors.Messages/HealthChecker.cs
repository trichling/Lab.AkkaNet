using System;
using Akka.Actor;

namespace Lab.AkkaNet.Banking.Actors.Messages
{
    public class HealthChecker : ReceiveActor
    {
        public HealthChecker()
        {
            Receive<Heartbeat>(heartbeat =>
            {
                Console.WriteLine("Received heartbeat from [{0}]: {1} {2}", Sender, heartbeat.SendTimestamp, heartbeat.AtmId);
                Sender.Tell(heartbeat);
            });
        }
       
    }

    public class StartHealthCheck
    {
    }

    public class SendHeartbeat
    {
    }

    public class Heartbeat
    {
        public long SendTimestamp { get; set; }
        public Guid AtmId { get; set; }
    }
}