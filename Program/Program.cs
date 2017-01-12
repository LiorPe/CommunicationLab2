using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

 namespace NetworkingLab

{
    class Program
    {
        static void Main(string[] args)
        {
            ChineseWhisperPlayer cwp = new ChineseWhisperPlayer();
            cwp.Run();

        }
        
    }
}
