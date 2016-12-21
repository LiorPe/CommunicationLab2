using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using CommunicationLab2;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            ChineseWhisperPlayer cwp = new ChineseWhisperPlayer();
            string serverName;
            IPAddress ip;
            short port;
            cwp.TryParseOfferMessage(cwp.GetOfferMessage(), out serverName, out ip, out port);
            cwp.Run();

        }

        



    }
}
