namespace Lab.AkkaNet.Banking.Actors.Messages
{
    public class Open
    {
        
        public Open(int number, decimal initialBalance)
        {
            Number = number;
            InitialBalance = initialBalance;
        }

        public int Number { get; }
        public decimal InitialBalance { get; }

    }

}