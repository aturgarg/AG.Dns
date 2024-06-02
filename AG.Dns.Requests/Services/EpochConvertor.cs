namespace AG.Dns.Requests.Services
{
    internal class EpochConvertor : IConverter
    {
        private readonly bool localTime;

        // Constructor for the Epoch class
        public EpochConvertor(bool localTime=false)
        {
            this.localTime = localTime;
        }

        // Method to create a new instance of Epoch
        //public static EpochGenerator New(bool localTime)
        //{
        //    return new EpochGenerator(localTime);
        //}

        // Parses the query which is an epoch and returns it in human readable form
        public List<string> Query(string query)
        {
            if (!long.TryParse(query, out long ts))
            {
                throw new ArgumentException("invalid epoch query");
            }

            if (ts >= 1e16 || ts <= -1e16)
            {
                // Nanoseconds
                ts /= 1000000000;
            }
            else if (ts >= 1e14 || ts <= -1e14)
            {
                // Microseconds
                ts /= 1000000;
            }
            else if (ts >= 1e11 || ts <= -3e10)
            {
                // Milliseconds
                ts /= 1000;
            }

            DateTime utc = DateTimeOffset.FromUnixTimeSeconds(ts).UtcDateTime;
            DateTime local = DateTimeOffset.FromUnixTimeSeconds(ts).DateTime;
            //DateTime utc =  DateTime.UnixEpoch.AddSeconds(ts).ToUniversalTime();
            //DateTime local = DateTime.UnixEpoch.AddSeconds(ts).ToLocalTime();

            //string outStr = $"{query} 1 TXT \"{utc:yyyy-MM-ddTHH:mm:ss.fffZ}\"";
            string outStr = $"{utc:yyyy-MM-ddTHH:mm:ss.fffZ}";
            if (localTime)
            {
                outStr += $" \"{local:yyyy-MM-ddTHH:mm:ss.fff}\"";
            }

            return new List<string> { outStr };
        }
    }
}
