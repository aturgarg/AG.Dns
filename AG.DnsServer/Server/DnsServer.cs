using AG.DnsServer.Handler;
using AG.DnsServer.Models;
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
        private IQueryHandler _queryHandler;

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
            _queryHandler = new QuestionHandler();
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
                        _queryHandler.Handle(remoteEndPoint.ToString(), message, question);

                        // store current IP address and Query ID.
                        StoreIPAndQueryId(remoteEndPoint, message);
                        SendResponse(remoteEndPoint, message);
                    }
                }
            }
        }
        
        private void SendResponse(EndPoint remoteEndPoint, DnsMessage message)
        {
            using (MemoryStream responseStream = new MemoryStream())
            {
                message.WriteToStream(responseStream);

                Interlocked.Increment(ref _responses);

                SendUdp(responseStream.GetBuffer(), 0, (int)responseStream.Position, remoteEndPoint);
            }
        }

        private void StoreIPAndQueryId(EndPoint remoteEndPoint, DnsMessage message)
        {
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
    }
}
