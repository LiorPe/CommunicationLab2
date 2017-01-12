using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationLab2
{
    public class Logger
    {
        Queue<string> logMessages;
        string _programName;

        public Logger(string progName)
        {
            logMessages = new Queue<string>();
            _programName = progName;
        }

        public void PrintLog(string message)
        {
            string record = GetCurrentTime() + " " + message;
            Console.WriteLine(record);
            logMessages.Enqueue(record);
        }
        
        private string GetCurrentTime()
        {
            return "[" + DateTime.Now.ToShortTimeString() + "]";
        }

        public void DumpLog()
        {
            string filePath = _programName + GetCurrentTime() + ".txt";
            StreamWriter sw = File.AppendText(filePath);
            while (logMessages.Count > 0)
                sw.WriteLine(logMessages.Dequeue());
            sw.Close();
        }
    }
}
