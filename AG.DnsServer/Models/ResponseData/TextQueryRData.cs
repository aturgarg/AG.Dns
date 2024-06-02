using AG.DnsServer.Extensions;

namespace AG.DnsServer.Models.ResponseData
{
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
            get { return (ushort)(Data.Length + 2); } 
        }

        public override void WriteToStream(Stream stream)
        {
            Data.WriteToStreamUnaltered(stream);
        }

        public override void Dump()
        {
            Console.WriteLine("NameServer:   {0}", Data);
        }
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

