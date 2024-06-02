using AG.DnsServer.Models;

namespace AG.DnsServer.Handler
{
    internal interface IQueryHandler
    {
        void Handle(string remoteEndPoint, DnsMessage message, Question question);
    }
}
