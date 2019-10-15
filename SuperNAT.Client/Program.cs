using System;

namespace SuperNAT.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Threading.Thread.Sleep(3000);
            HttpHandler.Start();
            Console.ReadKey();
        }
    }
}
