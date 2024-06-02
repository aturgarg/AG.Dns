namespace AG.DnsServer.Models.ResponseData
{
    public abstract class RData
    {
        public abstract void Dump();
        public abstract void WriteToStream(Stream stream);

        public abstract ushort Length { get; }

    }
}