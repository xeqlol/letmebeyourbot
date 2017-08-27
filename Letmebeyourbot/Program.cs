using System;

namespace Letmebeyourbot
{
    class Program
    {
        static void Main(string[] args)
        {
            Letmebeyourbot bot = new Letmebeyourbot();

            bot.Connect();
            Console.ReadLine();
            bot.Disconnect();
        }
    }
}
