using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            string s = "NameNetworking17";
            byte[] msg = System.Text.Encoding.ASCII.GetBytes(s);
            Console.WriteLine(msg.Length);
            Console.WriteLine(System.Text.ASCIIEncoding.Unicode.GetByteCount(s));
            Console.WriteLine(System.Text.ASCIIEncoding.ASCII.GetByteCount(s));

            byte [] intbytes = BitConverter.GetBytes(64);
            Console.WriteLine(intbytes.Length);

            Console.WriteLine(GetLocalIPAddress());
            byte[] ipBytes = System.Text.Encoding.ASCII.GetBytes(GetLocalIPAddress());
            Console.WriteLine(ipBytes.Length);

            Console.ReadKey();

        }


        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("Local IP Address Not Found!");
        }
    }
}
