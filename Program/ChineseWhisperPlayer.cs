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
        Boolean runProg;
        //to remove
        int cMinListeningTCPPort = 6001;
        int cMaxListeningTCPPort = 7000;
        string _programName = "fuckNetworking17";
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

        public ChineseWhisperPlayer()
        {
            runProg = true;
            _logger = new Logger(_programName);
            GenerateRandom();
            _meAsServerListeningPort = FindFirstAvailableListeningPort();
            _myUDPClient = new MyUDPClient(_programName);
            _myUDPServer = new MyUDPServer(_programName, _meAsServerListeningPort);
            _myTcpServer = new MyTCPServer(_localIp, _meAsServerListeningPort);
            _myTcpClient = new MyTCPClient();


        }

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
            }
            UpdateLog();


        }

        private void UpdateLog()
        {
            string log = "Program name:" + _programName;
            if (rx == Mode.On)
            {
                log += " ; " + _myClientTCPClientName + " is connected to our TCP server.";
            }
            else if (tx == Mode.On)
            {
                log += " ; " + "Our TCP client is connected to " + _myClientTCPServerName;
            }
            _logger.PrintLog(log);
        }

        //no-send/no-receive
        public void RunRXoffTXoff()
        {
            //IDO: send req msg
            //IDO: req msg rcvd -> send offer msg
            //IDO: no offers rcvd -> send req msg
            //IDO: incoming TCP connection attempt -> connect to client -> RXonTXoff
            //IDO: rcvd offer msg -> connect to server -> RXoffTXon

            SendRequestMessage();
            ListenToRequestMessages();
        }

        //updated
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
                        _myClientTCPClientName = requestMessageFromClient.ClientName;
                        _logger.PrintLog(String.Format("Server is connected to {0}.", _myClientTCPClientName));
                    }
                }

            }
        }

        //updated
        private void SendRequestMessage()
        {
            if (rx == Mode.Off)
            {
                //Console.WriteLine("No new request messages.");'
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
                            _myClientTCPServerName = oMsg.ServerName;
                            _logger.PrintLog(String.Format("Client is connected to {0}.", _myClientTCPServerName));
                        }
                    }

                }
            }
        }



        //yes-send/no-receive
        public void RunRXoffTXon()
        {
            //IDO: req msg rcvd -> send offer msg
            //IDO: input rcvd from user -> send msg to server
            //IDO: incoming TCP connection attempt -> connect to client -> RXonTXon
            ListenToRequestMessages();
            // If was the first link and client connected to me - stop threat taking user Input
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
                    Console.WriteLine("The client has ended the connection. Press Enter to exit the program.");
                }
                if (!IsThread)
                {
                    InputThread = new Thread(ReadMessageFromConsoleAndSendToServer);
                    InputThread.Start();
                    IsThread = true;
                }
            }
        }

        //no-send/yes-receive
        public void RunRXonTXoff()
        {
            //IDO: stop sending req msg/looking for servers
            //IDO: msg rcvd from client -> print msg to terminal
            string message;
            message = GetMessageFromClient();
            if (message != "")
                Console.WriteLine("Message received: " + message);
        }

        //yes-send/yes-receive
        public void RunRXonTXon()
        {
            //IDO: msg rcvd from client -> replace one char -> send new msg to server
            if (!_myTcpClient.IsConnected() || !_myTcpServer.IsConnected())
            {
                runProg = false;
                Console.WriteLine("The host and/or client has ended the connection. Press Enter to exit the program.");
                Console.ReadLine();
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

        public string GetMessageFromClient()
        {
            string message;
            bool isClientConnected = _myTcpServer.TryGetMessageFromClient(out message);
            if (!isClientConnected)
            {
                runProg = false;
                Console.WriteLine("The host and/or client has ended the connection. Press Enter to exit the program.");
                Console.ReadLine();
            }
            return message;
        }

        public void SendMessageToServer(string message)
        {
            if (!_myTcpClient.EnqueueMessageToSned(message))
            {
                runProg = false;
                Console.WriteLine("The host has ended the connection. Press Enter to exit the program.");
                Console.ReadLine();
            }
        }



        private void ReadMessageFromConsoleAndSendToServer()
        {
            //IDO: get console.readline and send the given msg in a loop
            while (runProg)
            {
                Console.WriteLine("Enter message:");
                string input = Console.ReadLine();
                SendMessageToServer(input); 
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


        #region Set First Connection

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


        #endregion 
        private int GenerateRandom()
        {
            //IDO:_randomNumber determined once per run
            Random r = new Random();
            return (int)r.Next(0, Int32.MaxValue);
        }
    }
}
