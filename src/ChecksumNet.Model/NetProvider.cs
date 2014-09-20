using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ChecksumNet.Model
{
    public class StateObject
    {
        // Client  socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 1024;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        //public StringBuilder sb = new StringBuilder();
    }


    public class NetProvider
    {
        public NetProvider()
        {
            IsServerActive = false;

            IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            LocalEP = new IPEndPoint(ipAddress, 4800);

        }

        public bool IsServerActive { get; set; }
        public EndPoint RemoteEP { get; set; }
        public EndPoint LocalEP { get; set; }

        public delegate void ProcessData(byte[] data);

        public event ProcessData onDataReceived;

        public void SetConnection()
        {
            if (!ActAsClient())
            {
                IsServerActive = true;
                ActAsServer();
            }
        }

        public void ActAsServer()
        {
            //Start server
            const int Port = 4800;
            var server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            Console.Write("Running server..." + Environment.NewLine);
            server.Bind(new IPEndPoint(IPAddress.Any, Port));

            while (IsServerActive)
            {
                var sender = new IPEndPoint(IPAddress.Any, 0);
                EndPoint tempRemoteEP = sender;
                var buffer = new byte[1000];
                //Recive message from anyone.
                //Server could crash here if there is another server
                //on the network listening at the same port.
                server.ReceiveFrom(buffer, ref tempRemoteEP);

                Console.Write("Server got '" + buffer[0] + "' from " + tempRemoteEP + Environment.NewLine);
                Console.Write("Sending '2' to " + tempRemoteEP + Environment.NewLine);

                //Replay to client
                server.SendTo(new byte[] {2}, tempRemoteEP);

                RemoteEP = tempRemoteEP;
                return; //TODO: ждать пока IsServerActive=false или выходить по времени
            }
        }

        public bool ActAsClient()
        {
            const int Port = 4800;
            //string serverIp = "";

            //Get all addresses
            string hostname = Dns.GetHostName();
            IPHostEntry allLocalNetworkAddresses = Dns.Resolve(hostname);
            EndPoint tempRemoteEP = null;

            //Walk thru all network interfaces.
            foreach (IPAddress ip in allLocalNetworkAddresses.AddressList)
            {
                var client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                //Bind on port 0. The OS will give some port between 1025 and 5000.
                client.Bind(new IPEndPoint(ip, 0));

                //Create endpoint, broadcast.
                var AllEndPoint = new IPEndPoint(IPAddress.Broadcast, Port);

                //Allow sending broadcast messages
                client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);

                //Send message to everyone on this network
                client.SendTo(new byte[] {1}, AllEndPoint);
                Console.Write("Client send '1' to " + AllEndPoint + Environment.NewLine);

                try
                {
                    //Create object for the server.
                    var sender = new IPEndPoint(IPAddress.Any, 0);
                    tempRemoteEP = sender;
                    var buffer = new byte[1000];

                    //Recieve from server. Don't wait more than 3000 milliseconds.
                    client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 3000);

                    //Recive message, save wherefrom in tempRemoteIp
                    client.ReceiveFrom(buffer, ref tempRemoteEP);
                    Console.Write("Client got '" + buffer[0] + "' from " + tempRemoteEP + Environment.NewLine);

                    //Get server IP (ugly)
                    //serverIp = tempRemoteEP.ToString().Split(":".ToCharArray(), 2)[0];

                    //Don't try any more networks

                    RemoteEP = tempRemoteEP;
                    return true;
                    //break;
                }
                catch (Exception)
                {
                    //Timout. No server answered. Try next network.
                }
            }

            //Console.Write("ServerIp: " + serverIp + Environment.NewLine);
            return false;
        }

        public void Send(byte[] data)
        {
            try
            {
                // Create a TCP/IP  socket.
                var sender = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream, ProtocolType.Tcp);

                // Connect the socket to the remote endpoint. Catch any errors.
                try
                {
                    sender.Connect(RemoteEP);

                    Console.WriteLine("Socket connected to {0}", sender.RemoteEndPoint);

                    int bytesSent = sender.Send(data);

                    // Release the socket.
                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();
                }
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0}", ane);
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException : {0}", se);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        // Thread signal.

        public void StartListening()
        {
            // Create a TCP/IP socket.
            var listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and listen for incoming connections.
            try
            {
                listener.Bind(LocalEP);
                listener.Listen(100);
                listener.BeginAccept(AcceptCallback,listener);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void AcceptCallback(IAsyncResult ar)
        {
            // Get the socket that handles the client request.
            var listener = (Socket) ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            // Create the state object.
            var state = new StateObject();
            state.workSocket = handler;
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
        }

        public void ReadCallback(IAsyncResult ar)
        {
            // Retrieve the state object and the handler socket
            // from the asynchronous state object.
            var state = (StateObject) ar.AsyncState;
            Socket handler = state.workSocket;

            // Read data from the client socket. 
            int bytesRead = handler.EndReceive(ar);
            
            handler.Shutdown(SocketShutdown.Both);
            handler.Close();

            var shortenBuffer = new byte[bytesRead];
            Array.Copy(state.buffer, 0, shortenBuffer, 0, bytesRead);
            onDataReceived(shortenBuffer);
        }
        
    }
}