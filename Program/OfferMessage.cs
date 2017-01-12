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
    public class  OfferMessage
    {
        public string ServerName { get; set; }
        public int RandNumber { get; set; }
        public IPAddress ServerIPAddress { get; set; }
        public short ServerListeningPort {get;set;}

        public OfferMessage(string serverName, int randNumber, IPAddress serverIPAdress, short serverListeningPort)
        {
            ServerName = serverName;
            RandNumber = randNumber;
            ServerIPAddress = serverIPAdress;
            ServerListeningPort = serverListeningPort;
        }
        public OfferMessage(Byte[] offerMessageInBytes)
        {
                    byte[] serverNameInBytes = new byte[16];
                    int lastByteRead = 0;
                    for (int i = 0; i < serverNameInBytes.Length; i++)
                    {
                        serverNameInBytes[i] = offerMessageInBytes[lastByteRead];
                        lastByteRead++;
                    }
                    byte[] randNumber = new byte[4];
                    for (int i = 0; i < randNumber.Length; i++)
                    {
                        randNumber[i] = offerMessageInBytes[lastByteRead];
                        lastByteRead++;
                    }
                    byte[] serverIpInBytes = new byte[4];
                    for (int i = 0; i < serverIpInBytes.Length; i++)
                    {
                        serverIpInBytes[i] = offerMessageInBytes[lastByteRead];
                        lastByteRead++;
                    }
                    byte[] serverPortInBytes = new byte[2];
                    for (int i = 0; i < serverPortInBytes.Length; i++)
                    {
                        serverPortInBytes[i] = offerMessageInBytes[lastByteRead];
                        lastByteRead++;
                    }

                    ServerName = System.Text.Encoding.Default.GetString(serverNameInBytes);
                    RandNumber = BitConverter.ToInt32(randNumber, 0);
                    ServerIPAddress = new IPAddress(serverIpInBytes);
                    ServerListeningPort = BitConverter.ToInt16(serverPortInBytes, 0);
        }
        public byte[] ToByteArray()
        {
            byte[] programNameInBytes = System.Text.Encoding.ASCII.GetBytes(ServerName);
            byte[] numberInBytes = BitConverter.GetBytes(RandNumber);
            byte[] localIPInBytes = ServerIPAddress.GetAddressBytes();
            byte[] listeningPortInBytes = BitConverter.GetBytes(ServerListeningPort);


            byte[] offerMessage = new byte[programNameInBytes.Length+ numberInBytes.Length+ localIPInBytes.Length+ listeningPortInBytes.Length];
            int lastByteWritten = 0;
            for (int i = 0; i < programNameInBytes.Length; i++)
            {
                offerMessage[lastByteWritten] = programNameInBytes[i];
                lastByteWritten++;
            }
            for (int i = 0; i < numberInBytes.Length; i++)
            {
                offerMessage[lastByteWritten] = numberInBytes[i];
                lastByteWritten++;

            }
            for (int i = 0; i < localIPInBytes.Length; i++)
            {
                offerMessage[lastByteWritten] = localIPInBytes[i];
                lastByteWritten++;

            }
            for (int i = 0; i < listeningPortInBytes.Length; i++)
            {
                offerMessage[lastByteWritten] = listeningPortInBytes[i];
                lastByteWritten++;

            }

            return offerMessage;
        }

        public static bool TryParseOfferMessage(byte[] receiveBytes , string myProgramName, int myID)
        {
            try
            {
                if (receiveBytes.Length == 26)
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
                    byte[] serverIpInBytes = new byte[4];
                    for (int i = 0; i < serverIpInBytes.Length; i++)
                    {
                        serverIpInBytes[i] = receiveBytes[lastByteRead];
                        lastByteRead++;
                    }
                    byte[] serverPortInBytes = new byte[2];
                    for (int i = 0; i < serverPortInBytes.Length; i++)
                    {
                        serverPortInBytes[i] = receiveBytes[lastByteRead];
                        lastByteRead++;
                    }

                    string serverName = System.Text.Encoding.Default.GetString(serverNameInBytes);
                    int randNum = BitConverter.ToInt32(randNumber, 0);
                    IPAddress localAddress = new IPAddress(serverIpInBytes);
                    short myServerPort = BitConverter.ToInt16(serverPortInBytes, 0);
                    if (!serverName.Contains("Networking17") || serverName==myProgramName)
                        return false;
                    if (randNum != myID)
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
            return String.Format("Server name:{0} , Random number:{1}, IP:{2}, Port:{3}", ServerName, RandNumber, ServerIPAddress, ServerListeningPort);
        }
    } 
}
