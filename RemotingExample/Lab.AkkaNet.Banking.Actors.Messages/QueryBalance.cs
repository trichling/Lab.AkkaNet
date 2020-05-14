namespace Lab.AkkaNet.Banking.Actors.Messages
{
    public class QueryBalance
    {
        public QueryBalance(int number)
        {
            Number = number;
        }
        public int Number { get; set; }

    }

}