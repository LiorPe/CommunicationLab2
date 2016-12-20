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

    public class Program
    {
        public enum Mode { On , Off};
        Mode rx = Mode.Off;
        Mode tx = Mode.Off;
        int cUDPBroadcatPort = 6000;
        int cMinListeningTCPPort = 6000;
        int cMaxListeningTCPPort = 7000;

        IPAddress localAddr = IPAddress.Parse("127.0.0.1");
        TcpListener tcpListener;

        TcpClient tcpClient;

        UdpClient udpClient;


        int _listeningPort;
        private char[] _requetMessage;
        string _serverIp;
        public void Run()
        {
            RunRXoffTXoff();
        }

        #region rx-off-tx-off
        public void RunRXoffTXoff()
        {
            while (true)
            {
                if (IsClienntConnectedToListeningPort())
                {
                    rx = Mode.On;
                    RunRXonTXoff();
                }
                if (FindServerPort())
                {
                    tx = Mode.On;
                    RunRXoffTXon();
                }
            }


        }

        private bool FindServerPort()
        {
            udpClient = new UdpClient(new IPEndPoint(IPAddress.Broadcast, cUDPBroadcatPort));
            IPAddress ipAddress = IPAddress.Any;
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, cUDPBroadcatPort);
            Byte[] sendBytes = Encoding.ASCII.GetBytes(GetRequestMessage());
            udpClient.Send(sendBytes, sendBytes.Length, ipEndPoint);
            udpClient.Client.ReceiveTimeout = 1000;
            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
            Byte[] receiveBytes = null;
            receiveBytes = udpClient.Receive(ref RemoteIpEndPoint);
            {
                if (receiveBytes != null)
                {
                    ParseOfferMessage();
                    ConnectToServer();
                    return true;
                }

            }
            return false;


        }

        private char[] GetRequestMessage()
        {
            throw new NotImplementedException();
        }

        private void ConnectToServer()
        {
            throw new NotImplementedException();
        }

        private void ParseOfferMessage()
        {
            throw new NotImplementedException();
        }

        private bool IsClienntConnectedToListeningPort()
        {
            _listeningPort = FindFirstAvailableListeningPort();
            tcpListener = new TcpListener(localAddr, _listeningPort);
            tcpListener.Server.ReceiveTimeout = 1000;
            tcpListener.Start();
            tcpClient = tcpListener.AcceptTcpClient();
            if (tcpClient != null)
            {

                string requestMessage = null;

                // Get a stream object for reading and writing
                NetworkStream stream = tcpClient.GetStream();
                Byte[] bytes = new Byte[256];

                int i;

                // Loop to receive all the data sent by the client.
                while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                {
                    // Translate data bytes to a ASCII string.
                    requestMessage = System.Text.Encoding.ASCII.GetString(bytes, 0, i);

                    byte[] msg = System.Text.Encoding.ASCII.GetBytes(GetOfferMessage());

                    // Send back a response.
                    stream.Write(msg, 0, msg.Length);
                }
            }
            return tcpClient == null;



        }

        private string GetOfferMessage()
        {
            throw new NotImplementedException();
        }

        private int FindFirstAvailableListeningPort()
        {
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            TcpConnectionInformation[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();

            for (int port = cMinListeningTCPPort; port < cMaxListeningTCPPort; port++)
            {
                if (tcpConnInfoArray.All(tcpi => (tcpi.LocalEndPoint.Port != port)))
                {
                    return port;
                }
            }
            return -1;
        
        }
        #endregion

        #region rx-off-tx-on
        public void RunRXoffTXon()
        {

        }
        #endregion

        #region rx-on-tx-off
        public void RunRXonTXoff()
        {

        }
        #endregion

        #region rx-on-tx-on
        public void RunRXonTXon()
        {

        }
        #endregion
    }
}
