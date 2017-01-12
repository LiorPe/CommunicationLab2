using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationLab2
{
    public class MyUDPServer
    {
        string _programName;
        int cUDPBroadcatPort = 6000;
        UdpClient _meAsClientUdpServer;
        short ListeningPort;
        IPAddress _localIp = GetLocalIPAddress();
        IPEndPoint ep;

        /// <summary>
        /// ctor for the MyUDPServer class
        /// </summary>
        /// <param name="progName">Running program name for logging purposes</param>
        /// <param name="port">Listening port for the UDP server</param>
        public MyUDPServer(string progName, short port)
        {
            _programName = progName;
            ListeningPort = port;
            ep = new IPEndPoint(IPAddress.Any, cUDPBroadcatPort);

        }

        /// <summary>
        /// Logic for UDP server init
        /// </summary>
        private void InitServer()
        {
            if (_meAsClientUdpServer != null)
                _meAsClientUdpServer.Close();
            _meAsClientUdpServer = new UdpClient(cUDPBroadcatPort);
            _meAsClientUdpServer.Client.ReceiveTimeout = 1000;
        }

        /// <summary>
        /// Gets the machine's local IP
        /// </summary>
        /// <returns>Local IP as IPAddress</returns>
        public static IPAddress GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip;
                }
            }
            throw new Exception("Local IP Address Not Found!");
        }

        /// <summary>
        /// Listens for UDP messages
        /// </summary>
        /// <param name="response">out param for message</param>
        /// <returns>true if message received</returns>
        public bool ListenToRequests(out byte[] response)
        {
            InitServer();
            try
             {
                response = _meAsClientUdpServer.Receive(ref ep);
                if (response != null)
                {
                    return true;
                }
                else
                {
                    response = null;
                    return false;
                }
            }
            catch (Exception e)
            {
                response = null;
                return false;
            }

        }

        /// <summary>
        /// Sends an offer message via UDP
        /// </summary>
        /// <param name="rMsg">the RequestMessage object to be sent</param>
        public void SendOffer(RequestMessage rMsg)
        {
            OfferMessage oMsg = new OfferMessage(_programName ,rMsg.RandNumber, _localIp, ListeningPort);
            var message = oMsg.ToByteArray();
            try
            {
                _meAsClientUdpServer.Send(message, message.Length, ep);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
