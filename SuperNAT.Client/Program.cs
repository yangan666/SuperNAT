using System;
using Topshelf;

namespace SuperNAT.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            HostFactory.Run(x =>
            {
                x.Service<ClientHandler>(s =>
                {
                    s.ConstructUsing(name => new ClientHandler());
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });
                x.RunAsLocalSystem();

                x.SetDescription("SuperNATClient");
                x.SetDisplayName("SuperNATClient");
                x.SetServiceName("SuperNATClient");
            });

            Console.ReadKey();
        }
    }
}
