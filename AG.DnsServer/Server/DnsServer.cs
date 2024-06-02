using AG.Dns.Requests;
using AG.Dns.Requests.Services;
using AG.DnsServer.Enums;
using AG.DnsServer.Models;
using AG.DnsServer.Models.ResponseData;
using AG.DnsServer.UDP;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace AG.DnsServer.Server
{
    internal class DnsServer
    {
        private IPAddress[] _defaultDns;
        private UdpListener _udpListener;        
        private long _requests;
        private long _responses;
        private long _nacks;

        private Dictionary<string, EndPoint> _requestResponseMap = new Dictionary<string, EndPoint>();
        private ReaderWriterLockSlim _requestResponseMapLock = new ReaderWriterLockSlim();
        private ushort port;
        private RequestResolver _requestResolver;

        internal DnsServer(ushort port)
        {
            this.port = port;
        }

        public void Initialize()
        {
            _udpListener = new UdpListener();
            _udpListener.Initialize(this.port);
            _udpListener.OnRequest += ProcessUdpRequest;
            _defaultDns = GetDefaultDNS().ToArray();
            _requestResolver = new RequestResolver();
        } 

        
        public void Start(CancellationToken ct)
        {
            _udpListener.Start();
            ct.Register(_udpListener.Stop);
        }

        private void ProcessUdpRequest(byte[] buffer, EndPoint remoteEndPoint)
        {
            DnsMessage message;
            if (!DnsProtocol.TryParse(buffer, out message))
            {
                Console.WriteLine("unable to parse message");
                return;
            }

            Interlocked.Increment(ref _requests);

            if (message.IsQuery())
            {
                if (message.Questions.Count == 1)
                {
                    foreach (Question question in message.Questions)
                    {
                        Console.WriteLine("{0} asked for {1} {2} {3}", remoteEndPoint.ToString(), question.Name, question.Class, question.Type);

                        int lastDotIndex = question.Name.LastIndexOf('.');
                        if (lastDotIndex == -1 || lastDotIndex == question.Name.Length - 1)
                        {
                            string errorResponse = "Invalid query format.";
                            //return Encoding.UTF8.GetBytes(errorResponse);
                        }
                        

                        string type = question.Name.Substring(lastDotIndex + 1);
                        string query = question.Name.Substring(0, lastDotIndex - 1);
                        IConverter converter = _requestResolver.Resolve(type);
                        var result = converter.Query(query);
                        var rdata = new TextQueryRData() { Data = result.First().Trim() };

                        //message.QR = true;
                        //message.AA = false;
                        //message.RA = false;
                        //message.RD = true;
                        //message.RCode = (byte)RCode.NOERROR;
                        //message.AuthenticatingData = false;
                      
                        Console.WriteLine(rdata.Data);

                        message.AdditionalCount = 0;
                        message.Additionals.Clear();

                        message.QR = true;
                        message.AA = true;
                        message.RA = false;
                        message.AnswerCount++;                        
                        message.Answers.Add(new ResourceRecord
                        {
                            Name = query,
                            Class = ResourceClass.IN,
                            Type = ResourceType.TXT,
                            TTL = 60,
                            RData = rdata
                        });                       


                        // store current IP address and Query ID.
                        try
                        {
                            string key = GetKeyName(message);
                            _requestResponseMapLock.EnterWriteLock();
                            if (!_requestResponseMap.ContainsKey(key))
                            {
                                _requestResponseMap.Add(key, remoteEndPoint);
                            }
                        }
                        finally
                        {
                            _requestResponseMapLock.ExitWriteLock();
                        }

                        using (MemoryStream responseStream = new MemoryStream())
                        {
                            message.WriteToStream(responseStream);

                            Interlocked.Increment(ref _responses);

                            SendUdp(responseStream.GetBuffer(), 0, (int)responseStream.Position, remoteEndPoint);
                        }
                    }
                }
            }
        }

        private string GetKeyName(DnsMessage message)
        {
            if (message.QuestionCount > 0)
            {
                return string.Format("{0}|{1}|{2}|{3}", message.QueryIdentifier, message.Questions[0].Class, message.Questions[0].Type, message.Questions[0].Name);
            }
            else
            {
                return message.QueryIdentifier.ToString();
            }
        }

        /// <summary>Send UDP response via UDP listener socket</summary>
        /// <param name="bytes"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <param name="remoteEndpoint"></param>
        private void SendUdp(byte[] bytes, int offset, int count, EndPoint remoteEndpoint)
        {
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.RemoteEndPoint = remoteEndpoint;
            args.SetBuffer(bytes, offset, count);

            _udpListener.SendToAsync(args);
        }

        /// <summary>Returns list of manual or DHCP specified DNS addresses</summary>
        /// <returns>List of configured DNS names</returns>
        private IEnumerable<IPAddress> GetDefaultDNS()
        {
            NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface adapter in adapters)
            {

                IPInterfaceProperties adapterProperties = adapter.GetIPProperties();
                IPAddressCollection dnsServers = adapterProperties.DnsAddresses;

                foreach (IPAddress dns in dnsServers)
                {
                    Console.WriteLine("Discovered DNS: ", dns);

                    yield return dns;
                }

            }
        }

        public void DumpHtml(TextWriter writer)
        {
            writer.WriteLine("DNS Server Status<br/>");
            writer.Write("Default Nameservers:");
            foreach (IPAddress dns in _defaultDns)
            {
                writer.WriteLine(dns);
            }
            writer.WriteLine("DNS Server Status<br/>");
        }
    }
}
