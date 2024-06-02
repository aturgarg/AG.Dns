using Microsoft.Extensions.Configuration;

namespace AG.DnsServer
{
    public static class DnsFacade
    {   
        private static Server.DnsServer _dnsServer; 

        public static void Run(string configFile, CancellationToken ct)
        {
            if (!File.Exists(configFile))
            {
                throw new FileNotFoundException(null, configFile);
            }

            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile(configFile, true, true)
                .Build();

            var serverConfig = configuration.Get<Config.ServerConfig>();

            _dnsServer = new Server.DnsServer(serverConfig.Server.DnsListener.Port);
            _dnsServer.Initialize();            
            _dnsServer.Start(ct);
            
            ct.WaitHandle.WaitOne();
        }        
    }
}
