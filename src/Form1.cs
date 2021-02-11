using System;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;
using ArduinoUploader;
using ArduinoUploader.Hardware;

namespace Bike_Uploader
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            checkedListBox1.SetItemChecked(0, true);
            button4.BackColor = Color.YellowGreen;
            button5.BackColor = Color.Red;
            this.Text = "Comyar's Arduino Uploader!";
            this.Icon = new Icon(Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory())) + "\\bin\\Debug\\icon.ico");
        }
        private void Form1_Load(object sender, EventArgs e)
        {
           
        }

        ArduinoModel model = ArduinoModel.UnoR3;
        static string port = "";
        static long baud = 9600;

        static string file = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory())) + "\\paths\\Bike_Speedometer\\Bike_Speedometer.ino";
        string inFile = file;
        string hexFile = "";

        string errs = "";
        string msg = "";

         private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
          
        }

        public void updateComPorts()
        {
           string[] aports = SerialPort.GetPortNames();
           checkedListBox2.Items.Clear();

            foreach (string port in aports)
            { 
                if (checkedListBox2.Items.Contains(port))
                {
                    return;
                }
                else
                {
                    checkedListBox2.Items.Add(port);
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
           updateComPorts();
        }

        private void checkedListBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        private void textBox8_TextChanged(object sender, EventArgs e)
        {
            inFile = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory())) + textBox8.Text;
        }

        public static string GenerateName(int len)
        {
            Random r = new Random(DateTime.Now.Second);
            string[] consonants = { "b", "c", "d", "f", "g", "h", "j", "k", "l", "m", "l", "n", "p", "q", "r", "s", "sh", "zh", "t", "v", "w", "x" };
            string[] vowels = { "a", "e", "i", "o", "u", "ae", "y" };
            string Name = "";
            Name += consonants[r.Next(consonants.Length)].ToUpper();
            Name += vowels[r.Next(vowels.Length)];
            int b = 2; //b tells how many times a new letter has been added. It's 2 right now because the first two letters are already in the name.
            while (b < len)
            {
                Name += consonants[r.Next(consonants.Length)];
                b++;
                Name += vowels[r.Next(vowels.Length)];
                b++;
            }

            return Name;
        }   

        private void button2_Click(object sender, EventArgs e)
        {
            string boardType = "";
            textBox3.BackColor = Color.YellowGreen;
            textBox3.Text = (string)("Compiling: " + file + ";" + model + ";" + port);
            button2.Text = "Compiling";


            foreach (string s in checkedListBox1.CheckedItems)
            {
                if (s == "Arduino Nano R3")
                {
                    model = ArduinoModel.UnoR3;
                    boardType = "uno";
                }

                else if (s == "Arduino Nano R3 (Old Bootloader)")
                {
                    model = ArduinoModel.NanoR3;
                    boardType = "nano:cpu=atmega328old";
                }

                else if (s == "Arduino Mega")
                {
                    model = ArduinoModel.Mega2560;
                    boardType = "mega";
                }

                else if (s == "Arduino Micro")
                {
                    model = ArduinoModel.Micro;
                    boardType = "micro";
                }

                else if (s == "Arduino Uno")
                {
                    model = ArduinoModel.UnoR3;
                    boardType = "uno";
                }

                else
                {
                    model = ArduinoModel.UnoR3;
                    boardType = "uno";
                }
            }

            foreach (string s in checkedListBox2.CheckedItems)
            {
                port = s;
            }

            try
            {
                ProcessStartInfo cmd = new ProcessStartInfo();

                cmd.FileName = "cmd.exe";
                cmd.WindowStyle = ProcessWindowStyle.Hidden;

                string name = GenerateName(8);
                hexFile = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory())) + "\\cache/" + name;

                cmd.Arguments = "/k cd " + Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory())) + "\\avr-g++\\arduino-cli\\bin"
                 + " & arduino-cli compile -b arduino:avr:" + boardType + " " + "\"" + @inFile + "\"" + " --build-path " + "\"" + @hexFile + "\"";

               file = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory())) + "\\cache\\" + name + "\\" + Path.GetFileName(inFile) + ".hex";
               Process.Start(cmd);
               Thread.Sleep(3500);
            }
            catch (Exception ex)
            {
                textBox3.BackColor = Color.Red;
                textBox3.Text = Convert.ToString(ex);
            }

            var uploader = new ArduinoSketchUploader(
              new ArduinoSketchUploaderOptions()
              {
                  FileName = file,
                  PortName = port,
                  ArduinoModel = model
              });

            textBox3.BackColor = Color.YellowGreen;
            textBox3.Text = (string)( "Uploading: " + file + ";" + model + ";" + port);
            button2.Text = "Uploading...";

            try
            {
                uploader.UploadSketch();
            }
            catch (Exception ex)
            {
                errs = ex.Message;
            }

             if (errs.Length > 0)
             {
                textBox3.BackColor = Color.Red;
                textBox3.Text = errs;
                button2.Text = "Failed";
             }
            else
            {
                textBox3.BackColor = Color.YellowGreen;
                textBox3.Text = "Uploaded";
                button2.Text = "Uploaded";
            }
            
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            try
            {
                baud = Convert.ToInt32(textBox2.Text);
            }
            catch (Exception ex)
            {
                textBox3.BackColor = Color.Red;
                textBox3.Text = Convert.ToString(ex);
            }
        }

        Thread t = null;
        SerialPort _serialPort;
        private delegate void SafeCallDelegate(string text);

        public void WriteTextSafe(string text)
        {
            if (textBox1.InvokeRequired)
            {
                var d = new SafeCallDelegate(WriteTextSafe);
                textBox1.Invoke(d, new object[] { text });
            }
            else
            {
                textBox1.AppendText(text);
            }
        }

        public void WriteErrorSafe(string text)
        {
            if (textBox3.InvokeRequired)
            {
                var d = new SafeCallDelegate(WriteErrorSafe);
                textBox3.Invoke(d, new object[] { text });
            }
            else
            {
                textBox3.AppendText(text);
            }
        }

        //CONNECT
        private void button4_Click(object sender, EventArgs e)
        {
            t = new Thread(new ThreadStart(MonitorThread));
            try
            {
                if (!t.IsAlive)
                {                               
                    t.IsBackground = true;
                    t.Start();
                }
                else
                {
                    t.IsBackground = true;
                }
            }
            catch (Exception ex)
            {
                textBox3.BackColor = Color.Red;
                textBox3.Text = Convert.ToString(ex);
            }
        }

        //DISCONNECT
        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                t.Abort();
                _serialPort.Close();
            }
            catch (Exception ex)
            {
                textBox3.BackColor = Color.Red;
                textBox3.Text = Convert.ToString(ex);
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
        }

        public void MonitorThread()
        {
            {
                foreach (string s in checkedListBox2.CheckedItems)
                {
                    port = s;
                }
                _serialPort = new SerialPort();
                try
                {
                    _serialPort.PortName = port; //Set your COM Port
                }
                catch (Exception ex)
                {
                    textBox3.BackColor = Color.Red;
                    WriteErrorSafe(Convert.ToString(ex));
                }

                _serialPort.BaudRate = (int)baud; //Set your Baud Rate

                try
                {
                    _serialPort.Open();
                }
                catch (Exception ex)
                {
                    textBox3.BackColor = Color.Red;
                    WriteErrorSafe(Convert.ToString(ex));
                }
                while (t.IsAlive)
                {
                    string a = " ";
                    try
                    {
                        a = _serialPort.ReadExisting();
                    }
                    catch (Exception ex)
                    {
                        textBox3.BackColor = Color.Red;
                        WriteErrorSafe(Convert.ToString(ex));
                    }
                    WriteTextSafe(a);
                    Thread.Sleep(100);
                }
            }
        }

        //UNUSED BOXES - 

        private void textBox3_Click(object sender, EventArgs e)
        {
    
        }


        private void Form1_Load_1(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
