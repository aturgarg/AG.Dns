﻿using AG.DnsServer;

namespace Dns.Server.Konsole
{
    internal class Program
    {
        private static CancellationTokenSource cts = new CancellationTokenSource();
        private static ManualResetEvent _exitTimeout = new ManualResetEvent(false);

        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World! I am a Dsn Server.");

            Console.CancelKeyPress += Console_CancelKeyPress;
           
            if (args.Length == 0)
            {
                args = new string[] { "./appsettings.json" };
            }
           
            DnsFacade.Run(args[0], cts.Token);  
            _exitTimeout.Set();
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Console.WriteLine("\r\nShutting Down");
            cts.Cancel();
            _exitTimeout.WaitOne(5000);
        }
    }
}
