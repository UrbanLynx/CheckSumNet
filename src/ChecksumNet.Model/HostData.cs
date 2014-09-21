using System.Net;

namespace ChecksumNet.Model
{
    public class HostData
    {
        public HostData()
        {
            Checksum = null;
        }
        public HostData(EndPoint ip)
        {
            IP = ip;
        }

        public bool FileReady { get; set; }
        public EndPoint IP { get; set; }
        public byte[] Checksum { get; set; }
        public string PcName { get; set; }
    }
}