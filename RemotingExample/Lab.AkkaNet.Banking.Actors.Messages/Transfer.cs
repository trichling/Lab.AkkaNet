namespace Lab.AkkaNet.Banking.Actors.Messages
{
    public class Transfer
    {

        public Transfer(int sourceAccountNumber, int targetAccountNumber, decimal amount)
        {
            SourceAccountNumber = sourceAccountNumber;
            TargetAccountNumber = targetAccountNumber;
            Amount = amount;
        }

        public int SourceAccountNumber { get; }
        public int TargetAccountNumber { get; }
        public decimal Amount { get; }
    }

}