using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.PeerToPeer;

namespace ChecksumNet.Model
{
    public class PeerEntry
    {
        public PeerName PeerName { get; set; }
        public IP2PService ServiceProxy { get; set; }
        public string DisplayString { get; set; }
        public bool CanConnect { get; set; }
        public string Checksum { get; set; }
    }
}
