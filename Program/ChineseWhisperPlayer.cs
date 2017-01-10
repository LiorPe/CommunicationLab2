using System;
using System.Collections.Generic;
using System.IO;
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

        UdpClient _meAsServerUdpClient;

        short _meAsServerListeningPort;
        private char[] _requestMessage;
        string _serverIp;

        IPAddress _localIp = GetLocalIPAddress();
        int _randomNumber;

        string _myServerName;
        IPAddress _myServerIp;
        short _myServerPort;

        Thread InputThread;
        Boolean IsThread = false;


        public void Run()
        {
            GenerateRandom();
            _meAsServerListeningPort = FindFirstAvailableListeningPort();
            while (true)
            {
                if(rx == Mode.Off && tx == Mode.Off)
                {
                    RunRXoffTXoff();
                }
                else if (rx == Mode.Off && tx == Mode.On)
                {
                    RunRXoffTXon();
                }
                else if (rx == Mode.On && tx == Mode.Off)
                {
                    RunRXonTXoff();
                }
                else if (rx == Mode.On && tx == Mode.On)
                {
                    RunRXonTXon();
                }
            }
        }

        private void GenerateRandom()
        {
            //IDO:_randomNumber determined once per run
            Random r = new Random();
            _randomNumber = r.Next(Int32.MinValue, Int32.MaxValue);
        }


        public void RunRXoffTXoff()
        {
            //IDO: send req msg
            //IDO: req msg rcvd -> send offer msg
            //IDO: no offers rcvd -> send req msg
            //IDO: incoming TCP connection attempt -> connect to client -> RXonTXoff
            //IDO: rcvd offer msg -> connect to server -> RXoffTXon
            
            if (IsClientConnectedToListeningPort())
                if (IncomingTCPConnection())
                    rx = Mode.On;
            else if (FindServerPort())
                tx = Mode.On;
        }

        public void RunRXoffTXon()
        {
            //IDO: req msg rcvd -> send offer msg
            //IDO: input rcvd from user -> send msg to server
            //IDO: incoming TCP connection attempt -> connect to client -> RXonTXon

            if (IsClientConnectedToListeningPort())
            {
                if (IncomingTCPConnection())
                {
                    rx = Mode.On;
                    InputThread.Abort();
                    InputThread.Join();
                    InputThread = null;
                    IsThread = false;
                }
            }
            else if (!IsThread)
            {
                InputThread = new Thread(SendUserInput);
                InputThread.Start();
                IsThread = true;
            }


        }

        public void RunRXonTXoff()
        {
            //IDO: stop sending req msg/looking for servers
            //IDO: msg rcvd from client -> print msg to terminal
            PrintUserMessage();
        }

        public void RunRXonTXon()
        {
            //IDO: msg rcvd from client -> replace one char -> send new msg to server
            string message = GetUserMessage();
            if(message != "")
            {
                string altered_message = AlterMessage(message);
                SendUserMessage(altered_message);
            }
        }

        private void SendUserInput()
        {
            //IDO: get console.readline and send the given msg in a loop
            while(true)
            {
                Console.WriteLine("Enter message:");
                string input = Console.ReadLine();
                if (_myClientTcpClient != null)
                {
                    NetworkStream stream = _myClientTcpClient.GetStream();
                    byte[] userMessage = System.Text.Encoding.ASCII.GetBytes(input);
                    //StreamWriter sw = new StreamWriter(stream);
                    //sw.WriteLine(userMessage);
                    stream.Write(userMessage, 0, userMessage.Length);
                    Console.WriteLine("Sent user input");
                    //stream.Close();
                }
            }
        }

        private bool IncomingTCPConnection()
        {
            ///IDO: check for TCP connection from clients
            ///IDO: incoming connection -> allow connection
            //IDO: no idea if this works
            if(_meAsServerTcpListener == null)
            {
                _meAsServerTcpListener = new TcpListener(localAddr, _meAsServerListeningPort);
                _meAsServerTcpListener.Server.ReceiveTimeout = 1000;
            }
            Thread.Sleep(1000);
            Console.WriteLine("Checking if any clients asked to connect.");
            _meAsServerTcpListener.Start();
            if (!_meAsServerTcpListener.Pending())
            {
                _meAsServerTcpListener.Stop();
                Console.WriteLine("No clients asked to connect.");
                return false;
            }
            _myClientTcpClient = _meAsServerTcpListener.AcceptTcpClient();
            /*Socket s = _meAsServerTcpListener.AcceptSocket();
            Console.WriteLine("Connection accepted from " + s.RemoteEndPoint);*/
            return true;
        }

        private void SendUserMessage(string altered_message)
        {
            //IDO: send msg
            //IDO: no idea if this works

            if (_myClientTcpClient != null)
            {
                NetworkStream stream = _myClientTcpClient.GetStream();
                /*Byte[] bytes = new Byte[256];
                while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                {
                    //IDO: print message to terminal?
                    Console.WriteLine("Got a request message");
                    // Translate data bytes to a ASCII string.
                    requestMessage = System.Text.Encoding.ASCII.GetString(bytes, 0, i);

                    byte[] offerMessage = GetOfferMessage();

                    // Send back a response.
                    //IDO: print message to terminal?
                    Console.WriteLine("Sent offer message");
                    stream.Write(offerMessage, 0, offerMessage.Length);
                }*/
                byte[] userMessage = System.Text.Encoding.ASCII.GetBytes(altered_message);

                // Send back a response.
                //IDO: print message to terminal?
                Console.WriteLine("Sent user message");
                stream.Write(userMessage, 0, userMessage.Length);
                //stream.Close();
            }
        }

        private string AlterMessage(string message)
        {
            //IDO: randomluy change one char in msg and return
            StringBuilder sb = new StringBuilder(message);
            Random r = new Random();
            int index = r.Next(0, message.Length-1);
            sb[index] = GetLetter();
            return sb.ToString();
        }

        public static char GetLetter()
        {
            string chars = "$%#@!*abcdefghijklmnopqrstuvwxyz1234567890?;:ABCDEFGHIJKLMNOPQRSTUVWXYZ^&";
            Random rand = new Random();
            int num = rand.Next(0, chars.Length - 1);
            return chars[num];

        }

        private string GetUserMessage()
        {
            //IDO: listen for msg -> return msg
            //IDO: no idea if this works

            string message = "";
            if (_myClientTcpClient != null)
            {
                Byte[] bytes = new Byte[256];

                // Get a stream object for reading and writing
                NetworkStream stream = _myClientTcpClient.GetStream();

                int i;

                // Loop to receive all the data sent by the client.
                while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                {
                    //IDO: print message to terminal?
                    Console.WriteLine("Got a user message");
                    // Translate data bytes to a ASCII string.
                    message = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                }
            }
            return message;
        }

        private void PrintUserMessage()
        {
            //IDO: listen for msg -> print msg
            string message = GetUserMessage();
            if(message != "")
                Console.WriteLine("Message received: " + message);
        }


        private bool FindServerPort() //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<continue comparison here
        {
            if (_meAsClientUdpClient == null)
            {
                _meAsClientUdpClient = new UdpClient();
                _meAsClientUdpClient.Connect(_localIp, cUDPBroadcatPort);
            }
            IPAddress ipAddress = IPAddress.Broadcast;
            Byte[] sendBytes = GetRequestMessage();
            //IDO:write the message to terminal as well?
            Console.WriteLine("Sending request messge");
            _meAsClientUdpClient.Send(sendBytes, sendBytes.Length);
            _meAsClientUdpClient.Client.ReceiveTimeout = 1000;
            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Broadcast, cUDPBroadcatPort);

            Byte[] receiveBytes = null;
            try
            {
                receiveBytes = _meAsClientUdpClient.Receive(ref RemoteIpEndPoint);
                {
                    if (receiveBytes != null)
                    {
                        int _myServerID;
                        if (TryParseOfferMessage(receiveBytes, out _myServerName, out _myServerID, out _myServerIp, out _myServerPort))
                        {
                            if(_myServerName != _programName && _myServerID == _randomNumber && _myServerName.Contains("Networking17"))
                            {
                                Console.WriteLine("Got an offer from: Server name {0}, IP {1}, port {2}", _myServerName, _myServerIp.ToString(), _myServerPort);
                                _meAsClientUdpClient.Close();
                                ConnectToServer(_myServerIp, _myServerPort);
                                return true;
                            }
                        }

                    }

                }

            }
            catch
            {

            }
            Console.WriteLine("Did not get an offer.");
            _meAsClientUdpClient.Close();
            return false;


        }

        private void ConnectToServer(IPAddress _myServerIp, short _myServerPort)
        {
            //IDO: no idea if this works
            if(_myClientTcpClient == null)
            {
                _myClientTcpClient = new TcpClient();
                Console.WriteLine("Connecting to server...");
                try
                {
                    _myClientTcpClient.Connect(_myServerIp, _myServerPort);
                }
                catch
                {
                    Console.WriteLine("Failed to connect.");
                }
            }

        }

        public bool TryParseOfferMessage(byte[] receiveBytes,out string serverName, out int idNum, out IPAddress myServerIp, out short myServerPort)
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
                    byte[] idNumInBytes = new byte[4];
                    for (int i = 0; i < idNumInBytes.Length; i++)
                    {
                        idNumInBytes[i] = receiveBytes[lastByteRead];
                        lastByteRead++;
                    }
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
                    idNum = BitConverter.ToInt32(idNumInBytes, 0);
                    myServerIp = new IPAddress(serverIpInBytes);
                    myServerPort = BitConverter.ToInt16(serverPortInBytes, 0);

                    return true;
                }

            }
            catch
            {
                serverName = String.Empty;
                idNum = -1;
                myServerIp = null;
                myServerPort = -1;
                return false;
            }
            serverName = String.Empty;
            idNum = -1;
            myServerIp = null;
            myServerPort = -1;
            return false;
        }

        private byte[] GetRequestMessage()
        {
            byte[] programNameInBytes  = System.Text.Encoding.ASCII.GetBytes(_programName);
            
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



        private bool IsClientConnectedToListeningPort()
        {
            if(_meAsServerUdpClient == null)
            {
                _meAsServerUdpClient = new UdpClient(cUDPBroadcatPort);
                _meAsServerUdpClient.Connect(_localIp, cUDPBroadcatPort);
                _meAsServerUdpClient.Client.ReceiveTimeout = 1000;
            }
            IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Broadcast, cUDPBroadcatPort);
            Console.WriteLine("Checking if any clients asked to connect.");
            try
            {
                Byte[] req = new Byte[20];
                req = _meAsServerUdpClient.Receive(ref remoteIpEndPoint);
                string hostName = Encoding.ASCII.GetString(req, 0, 16);
                //string randomNum = Encoding.ASCII.GetString(req, 16, 4);
                if (hostName.Contains("Networking17") && hostName != _programName)
                {
                    Console.WriteLine("Got a request message");
                    Byte[] offer = GetOfferMessage(req);
                    //Byte[] offer = Encoding.ASCII.GetBytes(_programName + randomNum + _localIp + _myServerPort);
                    _meAsServerUdpClient.Send(offer, 32);
                    Console.WriteLine("Sent offer message");
                    return true;
                }
            }
            catch
            {
                Console.WriteLine("No clients asked to connect.");
                return false;
            }
            return false;

        }

        public byte[] GetOfferMessage(Byte[] request)
        {
            string reqNum = Encoding.ASCII.GetString(request, 16, 4);
            byte[] programNameInBytes = System.Text.Encoding.ASCII.GetBytes(_programName);
            byte[] numberInBytes = System.Text.Encoding.ASCII.GetBytes(reqNum);
            byte[] localIPInBytes = _localIp.GetAddressBytes();
            byte[] listeningPortInBytes = BitConverter.GetBytes(_meAsServerListeningPort);


            byte[] offerMessage = new byte[programNameInBytes.Length+ numberInBytes.Length+ localIPInBytes.Length+ listeningPortInBytes.Length];
            int lastByteWritten = 0;
            for (int i = 0; i < programNameInBytes.Length; i++)
            {
                offerMessage[lastByteWritten] = programNameInBytes[i];
                lastByteWritten++;
            }
            for (int i = 0; i < numberInBytes.Length; i++)
            {
                offerMessage[lastByteWritten] = numberInBytes[i];
                lastByteWritten++;

            }
            for (int i = 0; i < localIPInBytes.Length; i++)
            {
                offerMessage[lastByteWritten] = localIPInBytes[i];
                lastByteWritten++;

            }
            for (int i = 0; i < listeningPortInBytes.Length; i++)
            {
                offerMessage[lastByteWritten] = listeningPortInBytes[i];
                lastByteWritten++;

            }

            return offerMessage;

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
