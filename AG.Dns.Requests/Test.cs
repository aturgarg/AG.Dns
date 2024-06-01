using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using DnsToysNET;
using DnsToysNET.Models;

namespace AG.Dns.Domain
{
    public class Test
    {
        private IDnsToys _dnsToys = new DnsToys();
        public Test() { }   

        public async Task TestQuery(string name)
        {
            // await GetIpAddress();
            await GetUnitConversion();
        }

        private async Task GetUnitConversion()
        {
           IDnsToysUnitEntry result =  await _dnsToys.UnitAsync(103.56, "kg", "g");
          Console.Write(result.ConvertedValue);
        }

        private async Task GetIpAddress()
        {
            IDnsToysIpEntry result = await _dnsToys.IpAsync();
            IPAddress requestingIP = result.RequestingIP;

            Console.WriteLine(requestingIP.ToString());
        }
    }
}
