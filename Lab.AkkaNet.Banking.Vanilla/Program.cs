using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lab.AkkaNet.Banking.Vanilla
{
    class Program
    {
        static void Main(string[] args)
        {
            TheMillionaresGame();
            TheFundraiser();
            TheLuckyLooserWithTheSadWinner();

            Console.ReadLine();
        }

        private static void TheFundraiser()
        {
            var bank = new Bank("Sparkasse Rheine");
            var bobsAccount = bank.Open(1, 0);

            var tasks = new List<Task>();
            for (int i = 0; i < 100000; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    bobsAccount.Deposit(1);
                }));
            }

            Task.WaitAll(tasks.ToArray());
        }

        private static void TheMillionaresGame()
        {
            var bank = new Bank("Sparkasse Rheine");
            var bobsAccount = bank.Open(1, 1000000);
            var samsAccount = bank.Open(2, 1000000);

            var bobToSam = Task.Run(() =>
            {
                for (int i = 0; i < 1000000; i++)
                {
                    bank.Transfer(1, 2, 1);
                }
            });

            var samToBob = Task.Run(() =>
            {
                for (int i = 0; i < 1000000; i++)
                {
                    bank.Transfer(2, 1, 1);
                }
            });

            Task.WaitAll(bobToSam, samToBob);
        }

        public static void TheLuckyLooserWithTheSadWinner()
        {
            var bank = new Bank("Sparkasse Rheine");
            var luckyLooser = bank.Open(1, 0);
            var sadWinner = bank.Open(2, 0);

            var tasks = new List<Task>();
            for (int i = 0; i < 100000; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    bank.Transfer(1, 2, 1);
                }));
            }

            Task.WaitAll(tasks.ToArray());
        }
    }
}