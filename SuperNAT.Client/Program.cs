using System;

namespace SuperNAT.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            HttpHandler.Start();
            Console.ReadKey();
        }
    }
}
