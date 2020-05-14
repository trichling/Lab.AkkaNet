namespace Lab.AkkaNet.Banking.Actors.Messages
{
    public class QueryAccountBalance
    {

        public QueryAccountBalance(int number)
        {
            Number = number;
        }

        public int Number { get; }

    }

}