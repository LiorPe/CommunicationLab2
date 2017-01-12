using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CommunicationLab2
{
    public class MyUDPClient
    {
        string _programName;
        int cUDPBroadcatPort = 6000;
        UdpClient _meAsClientUdpClient;
        IPEndPoint ep;

        public MyUDPClient(string progName)
        {
            _programName = progName;
            ep = new IPEndPoint(IPAddress.Any, cUDPBroadcatPort);
        }


        private void InitClient()
        {
            if (_meAsClientUdpClient != null)
                _meAsClientUdpClient.Close();
            _meAsClientUdpClient = new UdpClient();
            _meAsClientUdpClient.EnableBroadcast = true;
            _meAsClientUdpClient.DontFragment = true;
            _meAsClientUdpClient.Client.ReceiveTimeout = 1000;
        }

        public void SendRequestMessage(int randNum)
        {
            InitClient();
            var request = (new RequestMessage(_programName, randNum)).ToByteArray();

            _meAsClientUdpClient.Send(request, request.Length, new IPEndPoint(IPAddress.Broadcast, cUDPBroadcatPort));
        }

        public bool ListenToOffers(out byte[] response)
        {
            try
            {
                
                response = _meAsClientUdpClient.Receive(ref ep);
                if (response != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch(Exception e)
            {
                response = null;
                return false;
            }

        }

    }
}
