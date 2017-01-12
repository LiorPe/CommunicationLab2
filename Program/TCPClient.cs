using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationLab2
{
    public class TCPClient
    {
        string _programName;
        int cMinListeningTCPPort = 6001;
        int cMaxListeningTCPPort = 7000;
        TcpClient _myClientTcpClient;
    }
}
