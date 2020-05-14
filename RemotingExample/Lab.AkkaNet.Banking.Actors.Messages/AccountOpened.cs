using Lab.AkkaNet.Banking.Actors.Messages.ActorBase;

namespace Lab.AkkaNet.Banking.Actors.Messages
{
    public class AccountOpened : IEvent
    {
        
        public AccountOpened(int number, decimal initialBalance)
        {
            Number = number;
            InitialBalance = initialBalance;
        }

        public int Number { get; }
        public decimal InitialBalance { get; }

    }

}