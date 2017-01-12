using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkingLab
{
    public class MyTCPServer
    {
        public short ListeningPort { get; set; }
        TcpListener _tcpListener;
        TcpClient _ConnectedTCPClient;
        Thread InputThread;
        bool IsThread = false;
        public Queue<string> MessageQueue { get; set; }
        Mutex mutex = new Mutex();

        /// <summary>
        /// ctor for the MyTCPServer class
        /// </summary>
        /// <param name="ip">our local IP</param>
        /// <param name="listeningPort">listening port for server</param>
        public MyTCPServer(IPAddress ip ,short listeningPort)
        {
            int intPort = Convert.ToInt32(listeningPort);
            MessageQueue = new Queue<string>();

            _tcpListener = new TcpListener(ip, intPort);
            _tcpListener.Server.ReceiveTimeout = 1000;
            _tcpListener.Start();
        }


        /// <summary>
        /// Returns connection status
        /// </summary>
        /// <returns>true if connected</returns>
        public bool IsConnected()
        {
            return _ConnectedTCPClient.Connected; //_ConnectedTCPClient.Connected;
        }

        /// <summary>
        /// Listens to connection requests and attempts to accept them
        /// </summary>
        /// <returns>true if connection succeeded</returns>
        public bool CheckIfClientConnected()
        {
            Console.WriteLine("Checking if any clients asked to connect.");
            if (!_tcpListener.Pending())
            {
                return false;
            }
            else
            {
                try
                {
                    //_tcpListener.AcceptTcpClient();
                    _ConnectedTCPClient = _tcpListener.AcceptTcpClient();
                    if (!IsThread)
                        ReceiveLoop();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// init for the message receive thread
        /// </summary>
        private void ReceiveLoop()
        {
            InputThread = new Thread(_GetMessageFromClient);
            InputThread.Start();
            IsThread = true;
        }

        /// <summary>
        /// Listen to TCP messages
        /// </summary>
        private void _GetMessageFromClient()
        {
            while(_ConnectedTCPClient.Connected)
            {
                string msg = "";
                try
                {

                    var stream = _ConnectedTCPClient.GetStream();
                    int i;
                    Byte[] bytes = new Byte[256];
                    i = stream.Read(bytes, 0, bytes.Length);
                        msg = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                    stream.Flush();
                    if (msg != "")
                    {
                        mutex.WaitOne();
                        MessageQueue.Enqueue(msg);
                        mutex.ReleaseMutex();
                    }
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// Dequeue received messages
        /// </summary>
        /// <param name="message">out for message</param>
        /// <returns>true if message received</returns>
        public bool TryGetMessageFromClient(out string message)
        {
            mutex.WaitOne();
            if (MessageQueue.Count > 0)
            {
                message = String.Empty;
                message = MessageQueue.Dequeue();
            }
            else message = "";
            mutex.ReleaseMutex();
            return  _ConnectedTCPClient.Connected;

        }


    }
}
