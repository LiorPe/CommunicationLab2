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
    public class MyTCPClient
    {
       public IPAddress ServerIP { get; set; }
       public short ServerPort { get; set; }
       public Queue<string> MessageQueue { get; set; }
        Mutex mutex = new Mutex();

       TcpClient _tcpClient;
       Thread OutputThread;
        bool IsThread = false;


        public bool IsConnected()
       {
            return _tcpClient.Connected;
       }

        public bool TryConnectToServer(IPAddress serverIP, short serverPort)
        {
            ServerIP = serverIP;
            ServerPort = serverPort;
            _tcpClient = new TcpClient();
            try
            {
                _tcpClient.Connect(serverIP, serverPort);
                MessageQueue = new Queue<string>();
                if(!IsThread)
                    SendLoop();
                return true;

            }
            catch
            {
                return false;
            }

        }

        private void SendLoop()
        {
            OutputThread = new Thread(_SendMessage);
            OutputThread.Start();
            IsThread = true;
        }

        private void _SendMessage()
        {
            while(true)
            {
                mutex.WaitOne();
                if(MessageQueue.Count > 0)
                {
                    string message = MessageQueue.Dequeue();
                    mutex.ReleaseMutex();
                    NetworkStream stream = _tcpClient.GetStream();
                    byte[] userMessage = System.Text.Encoding.ASCII.GetBytes(message);
                    try
                    {
                        stream.Write(userMessage, 0, userMessage.Length);
                    }
                    catch
                    {
                    }

                }
            }
        }

        public bool SendMessage(string message)
        {
            mutex.WaitOne();
            MessageQueue.Enqueue(message);
            mutex.ReleaseMutex();
            if (!_tcpClient.Connected)
                return false;
            return true;
            /*NetworkStream stream = _tcpClient.GetStream();
            byte[] userMessage = System.Text.Encoding.ASCII.GetBytes(message);
            try
            {
                stream.Write(userMessage, 0, userMessage.Length);
                return true;
            }
            catch
            {
                return false;
            }*/
        }








    }
}
