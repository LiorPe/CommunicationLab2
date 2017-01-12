using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationLab2
{
    public class TCPServer
    {
        string _programName;
        int cMinListeningTCPPort = 6001;
        int cMaxListeningTCPPort = 7000;
        IPAddress localAddr = IPAddress.Parse("127.0.0.1");
        TcpListener _meAsServerTcpListener;
        short _meAsServerListeningPort;
    }
}
