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
    public class MyTCPServer
    {
        public short ListeningPort { get; set; }
        TcpListener _tcpListener;
        TcpClient _ConnectedTCPClient;
        Thread InputThread;
        bool IsThread = false;
        public Queue<string> MessageQueue { get; set; }
        Mutex mutex = new Mutex();
        bool _isClientConnected = false;

        public MyTCPServer(IPAddress ip ,short listeningPort)
        {
            int intPort = Convert.ToInt32(listeningPort);
            MessageQueue = new Queue<string>();

            _tcpListener = new TcpListener(ip, intPort);
            _tcpListener.Server.ReceiveTimeout = 1000;
            _tcpListener.Start();
        }

        public bool IsConnected()
        {
            return _isClientConnected; //_ConnectedTCPClient.Connected;
        }

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
                    _isClientConnected = true;
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        private void ReceiveLoop()
        {
            InputThread = new Thread(_GetMessageFromClient);
            InputThread.Start();
            IsThread = true;
        }

        private void _GetMessageFromClient()
        {
            while(_isClientConnected)
            {
                string msg = "";
                try
                {
                    mutex.WaitOne();
                    _isClientConnected = _ConnectedTCPClient.Connected;
                    mutex.ReleaseMutex();
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
                    _isClientConnected = false;
                }
            }
        }

        public bool TryGetMessageFromClient(out string message)
        {
            mutex.WaitOne();
            bool isClientConnect = _isClientConnected;
            if (MessageQueue.Count == 0)
            {
                message = String.Empty;
                message = MessageQueue.Dequeue();
            }
            else message = "";
            mutex.ReleaseMutex();
            return isClientConnect;

        }


    }
}
