using System.Net.PeerToPeer;

namespace ChecksumNet.Model
{
    // данные о равноправном участнике
    public class PeerEntry
    {
        public PeerName PeerName { get; set; } // данные о p2p соединении с пиром
        public IP2PService ServiceProxy { get; set; } // интерфейс для сообщения с пиром
        public string DisplayString { get; set; } // отображаемое имя пира
        public string Checksum { get; set; } // данные о контрольной сумме файла, выбранного пиром
    }
}