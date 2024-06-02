using System.Text.RegularExpressions;

namespace AG.Dns.Requests.Services
{
    internal class RandomGenerator : IConverter
    {
        public RandomGenerator()
        {
        }

        // Regular expression to match the query format
        private static readonly Regex queryFormat = new Regex(@"([0-9]+)-([0-9]+)", RegexOptions.Compiled);

        // Method to return a random number in the given range
        public List<string> Query(string query)
        {
            // Parse the query
            var match = queryFormat.Match(query);

            if (!match.Success || match.Groups.Count != 3)
            {
                throw new ArgumentException("invalid random query.");
            }

            if (!int.TryParse(match.Groups[1].Value, out int min) || !int.TryParse(match.Groups[2].Value, out int max))
            {
                throw new ArgumentException("invalid random query.");
            }

            var random = new Random();
            int v = random.Next(min, max + 1);

            //string result = $"{query} 1 TXT \"{v}\"";
            string result = $"{v}";

            return new List<string> { result };
        }

    }
}
