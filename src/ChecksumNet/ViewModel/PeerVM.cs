using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChecksumNet.Model;

namespace ChecksumNet.ViewModel
{
    public class PeerVM: ViewModelBase
    {
        public PeerEntry Peer;

        public PeerVM(PeerEntry peerEntry)
        {
            Peer = peerEntry;
        }

        public string PeerName
        {
            get { return Peer.DisplayString; }
        }

        public string Checksum
        {
            get { return Peer.Checksum; }
            set { Peer.Checksum = value; }
        }
    }
}
