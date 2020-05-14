using System;

namespace Lab.AkkaNet.Banking.Actors.ATM.Messges
{
    public class ConnectToBank
    {
        public Guid AtmId { get; set; }

        public string BankName { get; set; }
        
    }
}