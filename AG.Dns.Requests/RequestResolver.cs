using AG.Dns.Requests.Services;

namespace AG.Dns.Requests
{
    public class RequestResolver
    {

        public RequestResolver() { }

        public IConverter Resolve(string queryType)
        {
            return new UnitConverter();
        }
    }
}
