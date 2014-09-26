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

        public void DataUpdate()
        {
            OnPropertyChanged("PeerName");
            OnPropertyChanged("Checksum");
            OnPropertyChanged("IsChecksumsEqual");
        }

        public string PeerName
        {
            get { return Peer.DisplayString; }
            set { OnPropertyChanged("PeerName");}
        }

        public string Checksum
        {
            get { return Peer.Checksum; }
            set { OnPropertyChanged("Checksum");}
        }

        public bool IsChecksumsEqual
        {
            get
            {
                return Checksum == MainViewModel.Instance.LocalPeer.Checksum; 
            }
            set { OnPropertyChanged("IsChecksumsEqual"); }
        }
    }
}
