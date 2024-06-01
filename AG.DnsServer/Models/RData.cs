using System.Net;
using System.Text;
using AG.DnsServer.Extensions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AG.DnsServer.Models
{

    public abstract class RData
    {
        public abstract void Dump();
        public abstract void WriteToStream(Stream stream);

        public abstract ushort Length { get; }

    }

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
            byte[] bytes = this.Address.GetAddressBytes();
            stream.Write(bytes, 0, bytes.Length);
        }

        public override ushort Length
        {
            get { return 4; }
        }

        public override void Dump()
        {
            Console.WriteLine("Address:   {0}", this.Address.ToString());
        }
    }

    public class CNameRData : RData
    {
        public string Name { get; set; }

        public override ushort Length
        {
            // dots replaced by bytes 
            // + 1 segment prefix
            // + 1 null terminator
            get { return (ushort)(Name.Length + 2); }
        }

        public static CNameRData Parse(byte[] bytes, int offset, int size)
        {
            CNameRData cname = new CNameRData();
            cname.Name = DnsProtocol.ReadString(bytes, ref offset);
            return cname;
        }

        public override void WriteToStream(Stream stream)
        {
            Name.WriteToStream(stream);
        }

        public override void Dump()
        {
            Console.WriteLine("CName:   {0}", this.Name);
        }
    }

    public class DomainNamePointRData : RData
    {
        public string Name { get; set; }

        public static DomainNamePointRData Parse(byte[] bytes, int offset, int size)
        {
            DomainNamePointRData domainName = new DomainNamePointRData();
            domainName.Name = DnsProtocol.ReadString(bytes, ref offset);
            return domainName;
        }

        public override void WriteToStream(Stream stream)
        {
            Name.WriteToStream(stream);
        }

        public override ushort Length
        {
            // dots replaced by bytes 
            // + 1 segment prefix
            // + 1 null terminator
            get { return (ushort)(Name.Length + 2); }
        }

        public override void Dump()
        {
            Console.WriteLine("DName:   {0}", this.Name);
        }
    }

    public class NameServerRData : RData
    {
        public string Name { get; set; }

        public static NameServerRData Parse(byte[] bytes, int offset, int size)
        {
            NameServerRData nsRdata = new NameServerRData();
            nsRdata.Name = DnsProtocol.ReadString(bytes, ref offset);
            return nsRdata;
        }

        public override ushort Length
        {
            // dots replaced by bytes 
            // + 1 segment prefix
            // + 1 null terminator
            get { return (ushort)(Name.Length + 2); }
        }

        public override void WriteToStream(Stream stream)
        {
            this.Name.WriteToStream(stream);
        }


        public override void Dump()
        {
            Console.WriteLine("NameServer:   {0}", this.Name);
        }
    }

    public class StatementOfAuthorityRData : RData
    {

        public string PrimaryNameServer { get; set; }
        public string ResponsibleAuthoritativeMailbox { get; set; }
        public uint Serial { get; set; }
        public uint RefreshInterval { get; set; }
        public uint RetryInterval { get; set; }
        public uint ExpirationLimit { get; set; }
        public uint MinimumTTL { get; set; }

        public static StatementOfAuthorityRData Parse(byte[] bytes, int offset, int size)
        {
            StatementOfAuthorityRData soaRdata = new StatementOfAuthorityRData();
            soaRdata.PrimaryNameServer = DnsProtocol.ReadString(bytes, ref offset);
            soaRdata.ResponsibleAuthoritativeMailbox = DnsProtocol.ReadString(bytes, ref offset);
            soaRdata.Serial = DnsProtocol.ReadUint(bytes, ref offset).SwapEndian();
            soaRdata.RefreshInterval = DnsProtocol.ReadUint(bytes, ref offset).SwapEndian();
            soaRdata.RetryInterval = DnsProtocol.ReadUint(bytes, ref offset).SwapEndian();
            soaRdata.ExpirationLimit = DnsProtocol.ReadUint(bytes, ref offset).SwapEndian();
            soaRdata.MinimumTTL = DnsProtocol.ReadUint(bytes, ref offset).SwapEndian();
            return soaRdata;
        }

        public override ushort Length
        {
            // dots replaced by bytes 
            // + 1 segment prefix
            // + 1 null terminator
            get { return (ushort)(PrimaryNameServer.Length + 2 + ResponsibleAuthoritativeMailbox.Length + 2 + 20); }
        }

        public override void WriteToStream(Stream stream)
        {
            this.PrimaryNameServer.WriteToStream(stream);
            this.ResponsibleAuthoritativeMailbox.WriteToStream(stream);
            this.Serial.SwapEndian().WriteToStream(stream);
            this.RefreshInterval.SwapEndian().WriteToStream(stream);
            this.RetryInterval.SwapEndian().WriteToStream(stream);
            this.ExpirationLimit.SwapEndian().WriteToStream(stream);
            this.MinimumTTL.SwapEndian().WriteToStream(stream);
        }

        public override void Dump()
        {
            Console.WriteLine("PrimaryNameServer:               {0}", this.PrimaryNameServer);
            Console.WriteLine("ResponsibleAuthoritativeMailbox: {0}", this.ResponsibleAuthoritativeMailbox);
            Console.WriteLine("Serial:                          {0}", this.Serial);
            Console.WriteLine("RefreshInterval:                 {0}", this.RefreshInterval);
            Console.WriteLine("RetryInterval:                   {0}", this.RetryInterval);
            Console.WriteLine("ExpirationLimit:                 {0}", this.ExpirationLimit);
            Console.WriteLine("MinimumTTL:                      {0}", this.MinimumTTL);
        }
    }

    /*
     * 
     * Details of the Answer Section:
Name:

Uses a pointer to the question name (offset 12 in the packet), indicated by 0xC0 0x0C.
Type:

Set to 0x0010 for TXT records.
Class:

Set to 0x0001 for IN (Internet).
TTL:

Set to 60 seconds (0x0000003C).
RDLENGTH:

Length of the TXT RDATA. This includes the length byte plus the actual TXT data length.
TXT RDATA:

The TXT data itself is preceded by a length byte indicating the length of the TXT string.

     * */

    public class TextQueryRData : RData
    {
        public string Data { get; set; }

        public static TextQueryRData Parse(byte[] bytes, int offset, int size)
        {
            var tqRdata = new TextQueryRData();
            tqRdata.Data = DnsProtocol.ReadString(bytes, ref offset);
            return tqRdata;
        }

        public override ushort Length
        {
            // dots replaced by bytes 
            // + 1 segment prefix
            // + 1 null terminator
            get { return (ushort)(Data.Length + 2); } // TODO:
            //get { return (ushort)(Name.Length); }
            //get
            //{
            //    return (ushort)(this.Name.GetBytes().Length + 45);
            //}
        }

        public override void WriteToStream(Stream stream)
        {
            //Data.WriteToStream(stream);
            Data.WriteToStreamUnaltered(stream);
        }

        public override void Dump()
        {
            Console.WriteLine("NameServer:   {0}", this.Data);
        }
    }

}