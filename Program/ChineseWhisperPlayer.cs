using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CommunicationLab2
{

    public class ChineseWhisperPlayer
    {
        public enum Mode { On , Off};
        Mode rx = Mode.Off;
        Mode tx = Mode.Off;
        int cUDPBroadcatPort = 6000;
        int cMinListeningTCPPort = 6001;
        int cMaxListeningTCPPort = 7000;
        string _programName = "L&I_Networking17";
        IPAddress localAddr = IPAddress.Parse("127.0.0.1");
        TcpListener _meAsServerTcpListener;

        TcpClient _myClientTcpClient;

        UdpClient _meAsClientUdpClient;

        short _meAsServerListeningPort;
        private char[] _requetMessage;
        string _serverIp;

        IPAddress _localIp = GetLocalIPAddress();
        int _randomNumber;

        string _myServerName;
        IPAddress _myServerIp;
        short _myServerPort;


        public void Run()
        {
                        

            RunRXoffTXoff();
        }

        #region rx-off-tx-off
        public void RunRXoffTXoff()
        {
            _meAsServerListeningPort = FindFirstAvailableListeningPort();
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
            _meAsClientUdpClient = new UdpClient(new IPEndPoint(IPAddress.Any, cUDPBroadcatPort));
            IPAddress ipAddress = IPAddress.Broadcast;
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, cUDPBroadcatPort);
            Byte[] sendBytes = GetRequestMessage();
            Console.WriteLine("Sending request messge");
            _meAsClientUdpClient.Send(sendBytes, sendBytes.Length, ipEndPoint);
            _meAsClientUdpClient.Client.ReceiveTimeout = 1000;

            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
            Byte[] receiveBytes = null;
            try
            {
                receiveBytes = _meAsClientUdpClient.Receive(ref RemoteIpEndPoint);
                {
                    if (receiveBytes != null)
                    {
                        if (TryParseOfferMessage(receiveBytes, out _myServerName, out _myServerIp, out _myServerPort))
                        {
                            Console.WriteLine("Got an offer from: IP {0}, port {1}", _myServerIp.ToString(), _myServerPort);
                            _meAsClientUdpClient.Close();
                            ConnectToServer(_myServerIp, _myServerPort);
                            return true;
                        }

                    }

                }

            }
            catch
            {

            }
            Console.WriteLine("Did not got an offer.");
            _meAsClientUdpClient.Close();
            return false;


        }

        private void ConnectToServer(IPAddress _myServerIp, short _myServerPort)
        {
            throw new NotImplementedException();
        }

        public bool TryParseOfferMessage(byte[] receiveBytes,out string serverName, out IPAddress myServerIp, out short myServerPort)
        {
            try
            {
                if (receiveBytes.Length == 26)
                {
                    byte[] serverNameInBytes = new byte[16];
                    int lastByteRead = 0;
                    for (int i = 0; i < serverNameInBytes.Length; i++)
                    {
                        serverNameInBytes[i] = receiveBytes[lastByteRead];
                        lastByteRead++;
                    }
                    lastByteRead += 4;
                    byte[] serverIpInBytes = new byte[4];
                    for (int i = 0; i < serverIpInBytes.Length; i++)
                    {
                        serverIpInBytes[i] = receiveBytes[lastByteRead];
                        lastByteRead++;
                    }
                    byte[] serverPortInBytes = new byte[2];
                    for (int i = 0; i < serverPortInBytes.Length; i++)
                    {
                        serverPortInBytes[i] = receiveBytes[lastByteRead];
                        lastByteRead++;
                    }

                    serverName = System.Text.Encoding.Default.GetString(serverNameInBytes);
                     myServerIp = new IPAddress(serverIpInBytes);
                    myServerPort = BitConverter.ToInt16(serverPortInBytes, 0);

                    return true;
                }

            }
            catch
            {
                serverName = String.Empty;
                myServerIp = null;
                myServerPort = -1;
                return false;
            }
            serverName = String.Empty;
            myServerIp = null;
            myServerPort = -1;
            return false;
        }

        private byte[] GetRequestMessage()
        {
            byte[] programNameInBytes  = System.Text.Encoding.ASCII.GetBytes(_programName);
            Random r = new Random();
            _randomNumber = r.Next(Int32.MinValue,Int32.MaxValue);
            byte[] numberInBytes = BitConverter.GetBytes(_randomNumber);

            byte[] requestMessage = new byte[programNameInBytes.Length+ numberInBytes.Length];
            for (int i=0; i< programNameInBytes.Length; i++)
            {
                requestMessage[i] = programNameInBytes[i];
            }
            for (int i=0; i< numberInBytes.Length;i++)
            {
                requestMessage[programNameInBytes.Length + i] = numberInBytes[i];
            }

            return requestMessage;
        }



        private bool IsClienntConnectedToListeningPort()
        {
            _meAsServerTcpListener = new TcpListener(localAddr, _meAsServerListeningPort);
            _meAsServerTcpListener.Server.ReceiveTimeout = 1000;
            _meAsServerTcpListener.Start();
            Thread.Sleep(1000);
            Console.WriteLine("Checking if any client asked to connect.");
            if (!_meAsServerTcpListener.Pending())
            {
                _meAsServerTcpListener.Stop();
                Console.WriteLine("No client asked to connect.");

                return false;

            }

            _myClientTcpClient = _meAsServerTcpListener.AcceptTcpClient();
            if (_myClientTcpClient != null)
            {

                string requestMessage = null;

                // Get a stream object for reading and writing
                NetworkStream stream = _myClientTcpClient.GetStream();
                Byte[] bytes = new Byte[256];

                int i;

                // Loop to receive all the data sent by the client.
                while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                {

                    Console.WriteLine("Got a request message");
                    // Translate data bytes to a ASCII string.
                    requestMessage = System.Text.Encoding.ASCII.GetString(bytes, 0, i);

                    byte[] offerMessage = GetOfferMessage();

                    // Send back a response.
                    Console.WriteLine("Sent offer message");
                    stream.Write(offerMessage, 0, offerMessage.Length);
                }
            }
            return _myClientTcpClient != null && _myClientTcpClient.Client != null && _myClientTcpClient.Client.Connected; 



        }

        public byte[] GetOfferMessage()
        {
            byte[] programNameInBytes = System.Text.Encoding.ASCII.GetBytes(_programName);
            byte[] numberInBytes = BitConverter.GetBytes(_randomNumber);
            byte[] localIPInBytes = _localIp.GetAddressBytes();
            byte[] listeningPortInBytes = BitConverter.GetBytes(_meAsServerListeningPort);


            byte[] requestMessage = new byte[programNameInBytes.Length+ numberInBytes.Length+ localIPInBytes.Length+ listeningPortInBytes.Length];
            int lastByteWritten = 0;
            for (int i = 0; i < programNameInBytes.Length; i++)
            {
                requestMessage[lastByteWritten] = programNameInBytes[i];
                lastByteWritten++;
            }
            for (int i = 0; i < numberInBytes.Length; i++)
            {
                requestMessage[lastByteWritten] = numberInBytes[i];
                lastByteWritten++;

            }
            for (int i = 0; i < localIPInBytes.Length; i++)
            {
                requestMessage[lastByteWritten] = localIPInBytes[i];
                lastByteWritten++;

            }
            for (int i = 0; i < listeningPortInBytes.Length; i++)
            {
                requestMessage[lastByteWritten] = listeningPortInBytes[i];
                lastByteWritten++;

            }

            return requestMessage;

        }

        private short FindFirstAvailableListeningPort()
        {
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            TcpConnectionInformation[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();

            for (int port = cMinListeningTCPPort; port < cMaxListeningTCPPort; port++)
            {
                if (tcpConnInfoArray.All(tcpi => (tcpi.LocalEndPoint.Port != port)))
                {
                    return (short)port;
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
    }
}
