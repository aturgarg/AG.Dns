namespace AG.DnsServer.Config
{
    public class ServerOptions
    {
        public ZoneOptions Zone { get; set; }
        public DnsListenerOptions DnsListener { get; set; }
        public WebServerOptions WebServer { get; set; }
    }
}
