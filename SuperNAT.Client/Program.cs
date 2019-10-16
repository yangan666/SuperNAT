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
                x.Service<HttpHandler>(s =>
                {
                    s.ConstructUsing(name => new HttpHandler());
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
