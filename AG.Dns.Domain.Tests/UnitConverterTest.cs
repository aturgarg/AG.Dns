using AG.Dns.Requests.Services;

namespace AG.Dns.Domain.Tests
{
    public class UnitConverterTest
    {
        [Fact]
        public void Test1()
        {
            try
            {
                var units = new UnitConverter();
                var results = units.Query("10m-cm");
                foreach (var result in results)
                {
                    Console.WriteLine(result);
                    Assert.Contains("1000.00", result);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}