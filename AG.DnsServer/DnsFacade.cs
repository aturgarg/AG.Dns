using AG.Dns.Requests.Enums;
using AG.Dns.Requests.Services;
using AG.DnsServer.Models;
using AG.DnsServer.UDP;
using Microsoft.Extensions.Configuration;
//using Ninject;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Xml.Linq;

namespace AG.DnsServer
{
    public static class DnsFacade
    {
        //private static IKernel container = new StandardKernel();     
        private static Server.DnsServer _dnsServer; // resolver and delegated lookup for unsupported zones;
      

        public static void Run(string configFile, CancellationToken ct)
        {
            if (!File.Exists(configFile))
            {
                throw new FileNotFoundException(null, configFile);
            }

            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile(configFile, true, true)
                .Build();

            var appConfig = configuration.Get<Config.ServerConfig>();

            _dnsServer = new Server.DnsServer(appConfig.Server.DnsListener.Port);
            _dnsServer.Initialize();            
            _dnsServer.Start(ct);
            
            ct.WaitHandle.WaitOne();
        }

        /*
        private static void RunUdpListener()
        {
            UdpListener udpServer = new UdpListener();
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

            Console.WriteLine("DNS server started...");

            while (true)
            {
                try
                {

                }
                catch (SocketException se)
                {
                    Console.WriteLine($"SocketException: {se.Message}");
                    // Log the exception and continue listening for requests
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unexpected error: {ex.Message}");
                    // Log the exception and continue listening for requests
                }
            }
        }

        private static void RunUdpClient()
        {
            UdpClient udpServer = new UdpClient(8553);
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

            Console.WriteLine("DNS server started...");

            while (true)
            {
                try
                {
                    byte[] requestBytes = udpServer.Receive(ref remoteEndPoint);

                    byte[] responseBytes = HandleDnsRequest(requestBytes, remoteEndPoint);
                    udpServer.Send(responseBytes, responseBytes.Length, remoteEndPoint);
                    Console.WriteLine($"Handled request from {remoteEndPoint}");
                    // Problem : server stops. Should persists
                }
                catch (SocketException se)
                {
                    Console.WriteLine($"SocketException: {se.Message}");
                    // Log the exception and continue listening for requests
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unexpected error: {ex.Message}");
                    // Log the exception and continue listening for requests
                }
            }
        }

        private static void ProcessUdpRequest(byte[] buffer, EndPoint remoteEndPoint)
        {
            DnsMessage message;
            if (!DnsProtocol.TryParse(buffer, out message))
            {
                // TODO log bad message
                Console.WriteLine("unable to parse message");
                return;
            }

            //long _requests;
            //Interlocked.Increment(ref _requests);

            if (message.IsQuery())
            {
                if (message.Questions.Count > 0)
                {
                    foreach (Question question in message.Questions)
                    {
                        Console.WriteLine("{0} asked for {1} {2} {3}", remoteEndPoint.ToString(), question.Name, question.Class, question.Type);
                        IPHostEntry entry;
                    }
                }
            }
        }

        static byte[] HandleDnsRequest(byte[] requestBytes, EndPoint remoteEndPoint)
        {

            ProcessUdpRequest(requestBytes, remoteEndPoint);

            byte[] header = new byte[12];
            Array.Copy(requestBytes, 0, header, 0, 12);

            // Question Section (copy from request)
            int questionLength = requestBytes.Length - 12;
            byte[] question = new byte[questionLength];
            Array.Copy(requestBytes, 12, question, 0, questionLength);

            // Answer Section
            //byte[] answer = CreateAnswerSection(question);


            string query = Encoding.Unicode.GetString(question);
            string query2 = Encoding.ASCII.GetString(question);

            // Extract the type part after the last dot
            int lastDotIndex = query.LastIndexOf('.');
            if (lastDotIndex == -1 || lastDotIndex == query.Length - 1)
            {
                string errorResponse = "Invalid query format.";
                return Encoding.UTF8.GetBytes(errorResponse);
            }

            string type = query.Substring(lastDotIndex + 1);

            // Log the extracted type for demonstration purposes
            Console.WriteLine($"Extracted type: {type}");

            // Placeholder logic for handling different types
            if (type.Equals(QueryType.unit.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                // Actual logic to handle unit conversions should go here
                // For now, we'll just return a mock response
                //string responseString = $"Received unit query: {query}";
                var units = new UnitConverter();
                string conversionRequiredFor = query.Substring(0, lastDotIndex);
                List<string> responseString = units.Query(conversionRequiredFor);
                //return Encoding.UTF8.GetBytes(responseString);
                return ListStringToByteArray(responseString);
            }
            else
            {
                string errorResponse = $"Unknown query type: {type}";
                return Encoding.UTF8.GetBytes(errorResponse);
            }
        }

        public static byte[] ListStringToByteArray(List<string> stringList)
        {
            string combinedString = string.Join("\n", stringList);

            // Convert the combined string to byte[] using UTF-8 encoding
            byte[] byteArray = Encoding.UTF8.GetBytes(combinedString);
            return byteArray;
        }

        static byte[] HandleDnsRequest2(byte[] requestBytes)
        {
            byte[] header = new byte[12];
            Array.Copy(requestBytes, 0, header, 0, 12);

            // Modify flags to indicate a response
            header[2] = 0x81; // 10000001 in binary: Response, Opcode: Standard Query, Authoritative Answer, Not Truncated, Recursion Desired
            header[3] = 0x80; // 10000000 in binary: Recursion Available

            // Question Section (copy from request)
            int questionLength = requestBytes.Length - 12;
            byte[] question = new byte[questionLength];
            Array.Copy(requestBytes, 12, question, 0, questionLength);

            // Answer Section
            byte[] answer = CreateAnswerSection(question);

            // Construct response
            byte[] response = new byte[header.Length + question.Length + answer.Length];
            Array.Copy(header, 0, response, 0, header.Length);
            Array.Copy(question, 0, response, header.Length, question.Length);
            Array.Copy(answer, 0, response, header.Length + question.Length, answer.Length);

            return response;
        }

        static byte[] CreateAnswerSection(byte[] question)
        {
            // Answer Section
            // Assuming the question was a query for "example.com" and responding with "127.0.0.1"

            byte[] name = { 0xc0, 0x0c }; // Pointer to the domain name in the question section
            byte[] type = { 0x00, 0x01 }; // Type A (Host Address)
            byte[] clazz = { 0x00, 0x01 }; // Class IN (Internet)
            byte[] ttl = { 0x00, 0x00, 0x00, 0x3c }; // Time to live (60 seconds)
            byte[] dataLength = { 0x00, 0x04 }; // Length of the IP address (4 bytes)
            byte[] ipAddress = { 127, 0, 0, 1 }; // 127.0.0.1

            byte[] answer = new byte[name.Length + type.Length + clazz.Length + ttl.Length + dataLength.Length + ipAddress.Length];
            Array.Copy(name, 0, answer, 0, name.Length);
            Array.Copy(type, 0, answer, name.Length, type.Length);
            Array.Copy(clazz, 0, answer, name.Length + type.Length, clazz.Length);
            Array.Copy(ttl, 0, answer, name.Length + type.Length + clazz.Length, ttl.Length);
            Array.Copy(dataLength, 0, answer, name.Length + type.Length + clazz.Length + ttl.Length, dataLength.Length);
            Array.Copy(ipAddress, 0, answer, name.Length + type.Length + clazz.Length + ttl.Length + dataLength.Length, ipAddress.Length);

            return answer;
        }

        */

        
    }
}
