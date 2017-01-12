using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
namespace CommunicationLab2
{
    public class RequestMessage
    {
        public string ClientName { get; set; }
        public int RandNumber { get; set; }

        public RequestMessage(string clientName, int randNumber)
        {
            ClientName = clientName;
            RandNumber = randNumber;

        }
        public RequestMessage(Byte[] offerMessageInBytes)
        {
                    byte[] clientNameInBytes = new byte[16];
                    int lastByteRead = 0;
                    for (int i = 0; i < clientNameInBytes.Length; i++)
                    {
                        clientNameInBytes[i] = offerMessageInBytes[lastByteRead];
                        lastByteRead++;
                    }
                    byte[] randNumber = new byte[4];
                    for (int i = 0; i < randNumber.Length; i++)
                    {
                        randNumber[i] = offerMessageInBytes[lastByteRead];
                        lastByteRead++;
                    }

                    ClientName = System.Text.Encoding.Default.GetString(clientNameInBytes);
                    RandNumber = BitConverter.ToInt32(randNumber, 0);
        }
        public byte[] ToByteArray()
        {
            byte[] clientNameInBytes = System.Text.Encoding.ASCII.GetBytes(ClientName);
            byte[] randNumberInBytes = BitConverter.GetBytes(RandNumber);

            byte[] requestMessageInBytes = new byte[clientNameInBytes.Length+ randNumberInBytes.Length];
            int lastByteWritten = 0;
            for (int i = 0; i < clientNameInBytes.Length; i++)
            {
                requestMessageInBytes[lastByteWritten] = clientNameInBytes[i];
                lastByteWritten++;
            }
            for (int i = 0; i < randNumberInBytes.Length; i++)
            {
                requestMessageInBytes[lastByteWritten] = randNumberInBytes[i];
                lastByteWritten++;

            }
            return requestMessageInBytes;
        }

        public static bool TryParseRequestMessage(byte[] receiveBytes, string myProgramName)
        {
            try
            {
                if (receiveBytes.Length == 20)
                {
                    byte[] serverNameInBytes = new byte[16];
                    int lastByteRead = 0;
                    for (int i = 0; i < serverNameInBytes.Length; i++)
                    {
                        serverNameInBytes[i] = receiveBytes[lastByteRead];
                        lastByteRead++;
                    }
                    byte[] randNumber = new byte[4];
                    for (int i = 0; i < randNumber.Length; i++)
                    {
                        randNumber[i] = receiveBytes[lastByteRead];
                        lastByteRead++;
                    }
                    string serverName = System.Text.Encoding.Default.GetString(serverNameInBytes);
                    int randNum = BitConverter.ToInt32(randNumber, 0);
                    if (!serverName.Contains("Networking17") || serverName == myProgramName)
                        return false;
                    return true;
                }
            }
            catch
            {
            }
            return false;
        }

        public override string ToString()
        {
            return String.Format("Server name:{0} , Random number:{1}", ClientName, RandNumber);
        }
    }
}
