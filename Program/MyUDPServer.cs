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


        public MyUDPServer(string progName, short port)
        {
            _programName = progName;
            ListeningPort = port;
            ep = new IPEndPoint(IPAddress.Any, cUDPBroadcatPort);

        }


        private void InitServer()
        {
            if (_meAsClientUdpServer != null)
                _meAsClientUdpServer.Close();
            _meAsClientUdpServer = new UdpClient(cUDPBroadcatPort);
            _meAsClientUdpServer.Client.ReceiveTimeout = 1000;
        }

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
