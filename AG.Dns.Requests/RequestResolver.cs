using AG.Dns.Requests.Enums;
using AG.Dns.Requests.Services;

namespace AG.Dns.Requests
{
    public class RequestResolver
    {
        public RequestResolver() { }

        public IConverter Resolve(string queryType)
        {
            QueryType type;
            if (Enum.TryParse<QueryType>(queryType, true, out type))
            {
                switch (type)
                {
                    case QueryType.unit:
                        return new UnitConverter();
                    default:
                        throw new ArgumentException("Unable to identify converter required");
                }
            }
            else
            {
                Console.WriteLine("Conversion Failed for {0}", queryType);
                throw new Exception();
            }
        }
    }
}
