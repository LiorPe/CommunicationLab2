using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkingLab
{
    /// <summary>
    /// Logger class
    /// </summary>
    public class Logger
    {
        Queue<string> logMessages;
        string _programName;
        
        /// <summary>
        /// ctor for Logger class
        /// </summary>
        /// <param name="progName">Name of program running the logger for file naming purposes</param>
        public Logger(string progName)
        {
            logMessages = new Queue<string>();
            _programName = progName;
        }

        /// <summary>
        /// Print to screen and add to log queue a given message + timestamp
        /// </summary>
        /// <param name="message">Message to be printed and logged</param>
        public void PrintLog(string message)
        {
            string record = GetCurrentTime() + " " + message;
            Console.WriteLine(record);
            logMessages.Enqueue(record);
        }
        
        /// <summary>
        /// Gets the current time in a timestamp string
        /// </summary>
        /// <returns>[HH:MM:SS] timestamp string</returns>
        private string GetCurrentTime()
        {
            return "[" + DateTime.Now.ToShortTimeString() + "]";
        }

        /// <summary>
        /// Dumps the log queue to file
        /// </summary>
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
