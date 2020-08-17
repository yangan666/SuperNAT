using System;
using System.Runtime.InteropServices;
using Topshelf;

namespace SuperNAT.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            //Windows系统采用Topshelf
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
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
            }
            else
            {
                //其它系统不支持Topshelf
                new ClientHandler().Start();
            }

            Console.ReadKey();
        }
    }
}
