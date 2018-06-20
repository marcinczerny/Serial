using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace SerialConnect
{
    public partial class Form1 : Form
    {
        Thread thread;
        Serial serial;

        string leftInBuffer;
        string[] baundRate = { "300", "1200", "2400", "4800", "9600", "19200", "38400", "57600", "115200", "230400" };
        public Form1()
        {
            InitializeComponent();

            foreach (string baud in baundRate) {
                cmbBaudRate.Items.Add(baud);
            }

            cmbBaudRate.SelectedIndex = 4;
            serial = SerialFactory.CreateSerial(serialPort1);

            foreach (string port in Serial.GetPortNames())
            {
                cmbPort.Items.Add(port);
            }
            //    thread = new Thread(SendPacket, 0);
        }


        /// <summary>
        /// Delegat do zapewnienia bezpieczeństwa wątków przy odbieraniu wiadomości
        /// </summary>
        /// <param name="data"></param>
        delegate void delegatForSerialportMsg(string data);
        private void SetText(string dane)
        {
            bool sendingSuccesfull;
            if (this.textBox1.InvokeRequired)
            {
                delegatForSerialportMsg d = new delegatForSerialportMsg(SetText); //dodanie delegata do kolejki
                this.textBox1.BeginInvoke(d, new object[] { dane });  //tutej
            }
            else
            {
                //textBox1.Text += dane;
                bool result;

                
                dane = leftInBuffer + dane;
                leftInBuffer = dane;
                do {
                    

                    result = serial.DecodeMessage(ref dane);

                    if (result)
                    {
                        
                        txbTemperatura.Text = serial.temperature.ToString();
                        txbCisnienie.Text = serial.pressure.ToString();
                        txbWilgotnosc.Text = serial.humidity.ToString();
                    }

                        } while (result == true);
                //textBox1.Text += dane;



            }
        }


        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            SendClient("1");
        }

        private void serialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            string result = serial.ReadMessage();
            SetText(result);
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (serial.IsOpen == false)
            {
                string port = cmbPort.SelectedItem.ToString();
                port = port.Substring(0, port.IndexOf(' '));

                int baud;
                Int32.TryParse(cmbBaudRate.SelectedItem.ToString(), out baud);
                serial.Open(port, baud);

                btnConnect.Text = "Rozłącz";
            }
            else
            {
                serial.Close();
                btnConnect.Text = "Połącz";
            }


        }

        
        

        private void SendClient(string client)
        {
            serial.Write(client+"#");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SendClient("2");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SendClient("3");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            SendClient("4");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            SendClient("5");
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            serial.Close();
        }
    }
}
