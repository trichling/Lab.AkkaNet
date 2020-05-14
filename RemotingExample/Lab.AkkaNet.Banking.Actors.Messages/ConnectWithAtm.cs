using System;

namespace Lab.AkkaNet.Banking.Actors.Messages
{
    public class ConnectWithAtm
    {

        public Guid AtmId { get; set; }
        public string BankName { get; set; }

    }

}