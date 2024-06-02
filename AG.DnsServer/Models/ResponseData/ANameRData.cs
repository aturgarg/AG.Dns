using System.Net;

namespace AG.DnsServer.Models.ResponseData
{
    public class ANameRData : RData
    {
        public IPAddress Address
        {
            get;
            set;
        }

        public static ANameRData Parse(byte[] bytes, int offset, int size)
        {
            ANameRData aname = new ANameRData();
            uint addressBytes = BitConverter.ToUInt32(bytes, offset);
            aname.Address = new IPAddress(addressBytes);
            return aname;
        }

        public override void WriteToStream(Stream stream)
        {
            byte[] bytes = Address.GetAddressBytes();
            stream.Write(bytes, 0, bytes.Length);
        }

        public override ushort Length
        {
            get { return 4; }
        }

        public override void Dump()
        {
            Console.WriteLine("Address:   {0}", Address.ToString());
        }
    }
}
