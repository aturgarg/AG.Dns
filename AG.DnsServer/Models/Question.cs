using AG.DnsServer.Enums;
using AG.DnsServer.Extensions;

namespace AG.DnsServer.Models
{
    public class Question
    {
        public ResourceClass Class;
        public string Name;
        public ResourceType Type;

        public void WriteToStream(Stream stream)
        {
            byte[] name = this.Name.GetResourceBytes();
            stream.Write(name, 0, name.Length);

            // Type
            stream.Write(BitConverter.GetBytes(((ushort)(this.Type)).SwapEndian()), 0, 2);

            // Class
            stream.Write(BitConverter.GetBytes(((ushort)this.Class).SwapEndian()), 0, 2);
        }
    }
}