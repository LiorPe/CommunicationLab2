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
    /// <summary>
    /// Logic for main class for Chinese Whisper ("Broken Phone")
    /// </summary>
    public class ChineseWhisperPlayer
    {
        public enum Mode { On , Off};
        Mode rx = Mode.Off;
        Mode tx = Mode.Off;
        Boolean runProg;
        //to remove
        int cMinListeningTCPPort = 6001;
        int cMaxListeningTCPPort = 7000;
        string _programName = "L&I_Networking17";
        IPAddress localAddr = GetLocalIPAddress();

        short _meAsServerListeningPort;

        IPAddress _localIp = GetLocalIPAddress();

        Thread InputThread;
        Boolean IsThread = false;

        MyUDPClient _myUDPClient;
        MyUDPServer _myUDPServer;
        MyTCPClient _myTcpClient;
        MyTCPServer _myTcpServer;

        Logger _logger;

        string _myClientTCPClientName;
        string _myClientTCPServerName;

        bool statusChanged;

        /// <summary>
        ///ctor for ChineseWhisperPlayer class
        /// </summary>
        public ChineseWhisperPlayer()
        {
            runProg = true;
            statusChanged = false;
            _logger = new Logger(_programName);
            GenerateRandom();
            _meAsServerListeningPort = FindFirstAvailableListeningPort();
            _myUDPClient = new MyUDPClient(_programName);
            _myUDPServer = new MyUDPServer(_programName, _meAsServerListeningPort);
            _myTcpServer = new MyTCPServer(_localIp, _meAsServerListeningPort);
            _myTcpClient = new MyTCPClient();


        }

        /// <summary>
        /// Main running loop
        /// </summary>
        public void Run()
        {
            while (runProg)
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
                UpdateLog();
            }
            _logger.PrintLog("The host and/or client has ended the connection. Press Enter to exit the program.");
            Console.ReadLine();

        }

        /// <summary>
        /// Print status message to screen and add to logger
        /// </summary>
        private void UpdateLog()
        {
            if(statusChanged)
            {
                string log = "Program name:" + _programName;
                if (rx == Mode.Off)
                {
                    log += "No clients are connected to our TCP server;";
                }
                else if(rx == Mode.On)
                {
                    log += " ; " + _myClientTCPClientName + " is connected to our TCP server;";
                }

                if (tx == Mode.Off)
                {
                    log += "Our TCP client is not connected to any servers;";
                }
                else if (tx == Mode.On)
                {
                    log += " ; " + "Our TCP client is connected to " + _myClientTCPServerName + ";";
                }
                _logger.PrintLog(log);
                statusChanged = false;
            }
        }

        /// <summary>
        /// Function for RXoffTXoff state
        /// </summary>
        public void RunRXoffTXoff()
        {

            SendRequestMessage();
            ListenToRequestMessages();
        }

        /// <summary>
        /// Listen to UDP request messages
        /// </summary>
        private void ListenToRequestMessages()
        {
            byte[] requestMessageInBytes;

            if (_myUDPServer.ListenToRequests(out requestMessageInBytes))
            {
                if (RequestMessage.TryParseRequestMessage(requestMessageInBytes, _programName))
                {
                    RequestMessage requestMessageFromClient = new RequestMessage(requestMessageInBytes);
                    _logger.PrintLog(String.Format("Got request message: {0}", requestMessageFromClient));
                    _myUDPServer.SendOffer(requestMessageFromClient);
                    Thread.Sleep(1000);
                    if (_myTcpServer.CheckIfClientConnected())
                    {
                        rx = Mode.On;
                        statusChanged = true;
                        _myClientTCPClientName = requestMessageFromClient.ClientName;
                    }
                }

            }
        }

        /// <summary>
        /// send a UDP request message
        /// </summary>
        private void SendRequestMessage()
        {
            if (rx == Mode.Off)
            {
                byte[] responseForRequestMessage;
                int randNumber = GenerateRandom();
                _myUDPClient.SendRequestMessage(randNumber);
                if (_myUDPClient.ListenToOffers(out responseForRequestMessage))
                {
                    if (OfferMessage.TryParseOfferMessage(responseForRequestMessage, _programName, randNumber))
                    {
                        OfferMessage oMsg = new OfferMessage(responseForRequestMessage);
                        _logger.PrintLog(String.Format("Got offer message: {0}", oMsg));
                        if (_myTcpClient.TryConnectToServer(oMsg.ServerIPAddress, oMsg.ServerListeningPort))
                        {
                            tx = Mode.On;
                            statusChanged = true;
                            _myClientTCPServerName = oMsg.ServerName;
                        }
                    }

                }
            }
        }
        

        /// <summary>
        /// Function for RXoffTXon state
        /// </summary>
        public void RunRXoffTXon()
        {
            ListenToRequestMessages();
            if (rx == Mode.On)
            {
                if (InputThread != null)
                {
                    InputThread.Abort();
                    InputThread.Join();
                    InputThread = null;
                    IsThread = false;
                }
            }
            else
            {
                if(!_myTcpClient.IsConnected())
                {
                    runProg = false;
                    InputThread.Abort();
                    InputThread.Join();
                    InputThread = null;
                }
                if (!IsThread)
                {
                    InputThread = new Thread(ReadMessageFromConsoleAndSendToServer);
                    InputThread.Start();
                    IsThread = true;
                }
            }
        }


        /// <summary>
        /// Function for RXonTXoff state
        /// </summary>
        public void RunRXonTXoff()
        {
            string message;
            message = GetMessageFromClient();
            if (message != "")
                Console.WriteLine("Message received: " + message);
        }


        /// <summary>
        /// Function for RXonTXon state
        /// </summary>
        public void RunRXonTXon()
        {
            if (!_myTcpClient.IsConnected() || !_myTcpServer.IsConnected())
            {
                runProg = false;
            }
            string message = GetMessageFromClient();
            if (message != "")
            {
                Console.WriteLine("Message received: " + message);
                string altered_message = AlterMessage(message);
                Console.WriteLine("New message: " + altered_message);
                SendMessageToServer(altered_message);
            }
        }

        /// <summary>
        /// get a user message from TCP server
        /// </summary>
        /// <returns></returns>
        public string GetMessageFromClient()
        {
            string message;
            bool isClientConnected = _myTcpServer.TryGetMessageFromClient(out message);
            if (!isClientConnected)
            {
                runProg = false;
            }
            return message;
        }

        /// <summary>
        /// send a user message to TCP server
        /// </summary>
        /// <param name="message"></param>
        public void SendMessageToServer(string message)
        {
            if (!_myTcpClient.EnqueueMessageToSned(message))
            {
                runProg = false;
            }
        }


        /// <summary>
        /// read user input and send to TCP server
        /// </summary>
        private void ReadMessageFromConsoleAndSendToServer()
        {
            while (runProg)
            {
                Console.WriteLine("Enter message:");
                string input = Console.ReadLine();
                SendMessageToServer(input); 
            }
        }

        /// <summary>
        /// Alter a single character in a given string
        /// </summary>
        /// <param name="message">original string</param>
        /// <returns>altared string</returns>
        private string AlterMessage(string message)
        {
            StringBuilder sb = new StringBuilder(message);
            Random r = new Random();
            int index = r.Next(0, message.Length-1);
            sb[index] = GetLetter();
            return sb.ToString();
        }

        /// <summary>
        /// get a random character
        /// </summary>
        /// <returns>random character</returns>
        public static char GetLetter()
        {
            string chars = "$%#@!*abcdefghijklmnopqrstuvwxyz1234567890?;:ABCDEFGHIJKLMNOPQRSTUVWXYZ^&";
            Random rand = new Random();
            int num = rand.Next(0, chars.Length - 1);
            return chars[num];

        }


        #region Set First Connection

        /// <summary>
        /// find first available port within a set range
        /// </summary>
        /// <returns>available port</returns>
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

        /// <summary>
        /// get local IP address of machine
        /// </summary>
        /// <returns>local IP address</returns>
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


        #endregion 

        /// <summary>
        /// generate a random number within a set range
        /// </summary>
        /// <returns>random number</returns>
        private int GenerateRandom()
        {
            Random r = new Random();
            return (int)r.Next(0, Int32.MaxValue);
        }
    }
}
