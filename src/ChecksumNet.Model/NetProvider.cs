using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChecksumNet.Model
{
    public class NetProvider
    {
        public void SetConnection()
        {
            if( string.IsNullOrEmpty(ActAsClient()))
                ActAsServer();
        }

        public void ActAsServer()
        {
            //Start server
            const int Port = 4800;
            Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            Console.Write("Running server..." + Environment.NewLine);
            server.Bind(new IPEndPoint(IPAddress.Any, Port));

            while (true)
            {
                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                EndPoint tempRemoteEP = (EndPoint)sender;
                byte[] buffer = new byte[1000];
                //Recive message from anyone.
                //Server could crash here if there is another server
                //on the network listening at the same port.
                server.ReceiveFrom(buffer, ref tempRemoteEP);

                Console.Write("Server got '" + buffer[0] + "' from " + tempRemoteEP.ToString() + Environment.NewLine);
                Console.Write("Sending '2' to " + tempRemoteEP.ToString() + Environment.NewLine);

                //Replay to client
                server.SendTo(new byte[] { 2 }, tempRemoteEP);
            }
        }

        public string ActAsClient()
        {
            const int Port = 4800;
            string serverIp = "";

            //Get all addresses
            string hostname = System.Net.Dns.GetHostName();
            IPHostEntry allLocalNetworkAddresses = Dns.Resolve(hostname);

            //Walk thru all network interfaces.
            foreach (IPAddress ip in allLocalNetworkAddresses.AddressList)
            {
                Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                //Bind on port 0. The OS will give some port between 1025 and 5000.
                client.Bind(new IPEndPoint(ip, 0));

                //Create endpoint, broadcast.
                IPEndPoint AllEndPoint = new IPEndPoint(IPAddress.Broadcast, Port);

                //Allow sending broadcast messages
                client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);

                //Send message to everyone on this network
                client.SendTo(new byte[] { 1 }, AllEndPoint);
                Console.Write("Client send '1' to " + AllEndPoint.ToString() + Environment.NewLine);

                try
                {
                    //Create object for the server.
                    IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                    EndPoint tempRemoteEP = (EndPoint)sender;
                    byte[] buffer = new byte[1000];

                    //Recieve from server. Don't wait more than 3000 milliseconds.
                    client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 3000);

                    //Recive message, save wherefrom in tempRemoteIp
                    client.ReceiveFrom(buffer, ref tempRemoteEP);
                    Console.Write("Client got '" + buffer[0] + "' from " + tempRemoteEP.ToString() + Environment.NewLine);

                    //Get server IP (ugly)
                    serverIp = tempRemoteEP.ToString().Split(":".ToCharArray(), 2)[0];

                    //Don't try any more networks
                    break;
                }
                catch
                {
                    //Timout. No server answered. Try next network.
                }
            }

            Console.Write("ServerIp: " + serverIp + Environment.NewLine);
            return serverIp;
        }

    }
}
