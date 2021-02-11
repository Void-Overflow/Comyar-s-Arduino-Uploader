using System;
using System.Windows.Forms;
using ArduinoUploader;
using ArduinoUploader.Hardware;

namespace Bike_Uploader
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
            //InitializeComponent();
        }
    }
}
