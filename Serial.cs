using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using System.IO.Ports;
using System.IO;
using System.Globalization;


namespace SerialConnect
{
    public struct messageDecoded
    {
        public float temperature;
        public float pressure;
        public float humidity;
    }


    public class Serial
    {
        private SerialPort UART;
        private long baudRate;
        private string portName;

        public float temperature;
        public float pressure;
        public float humidity;
        
        public string IncommingMessage;
        public messageDecoded decodedMessage;

        string leftInBuffer;

        public bool IsOpen
        {
            get { return UART.IsOpen; }
        }

        public void getSerial(SerialPort serial) {
            this.UART = serial;
        }

        public Serial()
        {
            decodedMessage = new messageDecoded();
        }
        public void Open(string portName,int baud)
        {
            UART.PortName = portName;
            UART.BaudRate = baud;
            if (!UART.IsOpen)
            {
                try
                {
                    UART.Open();
                }
                catch (IOException)
                {

                }
            }
        }
        public void Close()
        {
            try {
                UART.Close();
            }
            catch (IOException)
            {

            }
            
        }

        public string ReadMessage()
        {
            string result;
            if (UART.IsOpen)
            {

                result = UART.ReadExisting();
                //   int leftInBufor = BytesToRead;

                // while (leftInBufor > 0)
                //{
                //   leftInBufor = GetDataFromSerial();
                //}
                //}
                return result;

            }

            return null;
        }
        public void Write(string msg)
        {
            try
            {
                UART.Write(msg);
            }
            catch (InvalidOperationException)
            {

            }
        }
        public static List<string> GetPortNames() {
            List<string> result = new List<string>();

            using (var searcher = new ManagementObjectSearcher
                ("SELECT * FROM WIN32_SerialPort"))
            {
                string[] portnames = SerialPort.GetPortNames();
                var ports = searcher.Get().Cast<ManagementBaseObject>().ToList();
                var tList = (from n in portnames
                             join p in ports on n equals p["DeviceID"].ToString()
                             select n + " - " + p["Caption"]).ToList();

                foreach (string t in tList) {
                    result.Add(t);
                }
            }

            return result;
        }

        
            public bool DecodeMessage(ref string message)
        {

            message = leftInBuffer + message;
            
            int endFrame = message.IndexOf('#');
            if(endFrame == -1)
            {
                
                return false; 
            }

            int beginFrame = message.IndexOf('$');
            if(beginFrame == -1 || beginFrame > endFrame)
            {
                message = message.Substring(endFrame+1);
                return true;
            }
            try
            {
                string result = message.Substring(beginFrame, endFrame - beginFrame + 1);
                string tempString = result.Substring(1, result.IndexOf('%') - 1);
                string presString = result.Substring(result.IndexOf('%') + 1, result.IndexOf('^') - (result.IndexOf('%') + 1));
                string humString = result.Substring(result.IndexOf('^') + 1, result.Length - 1 - (result.IndexOf('^') + 1));

                this.humidity = float.Parse(humString, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture);
                this.pressure = float.Parse(presString, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture);
                this.temperature = float.Parse(tempString, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture);
            } catch(ArgumentOutOfRangeException)
            {
                message = message.Substring(endFrame + 1);
                return true;
            }

            

            message = message.Substring(endFrame + 1);
            
            return true;
        }

        
    }
}
