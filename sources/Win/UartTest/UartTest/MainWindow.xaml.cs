using System;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO.Ports;
using System.Management;
using sd=System.Drawing;
using System.Runtime.InteropServices;

namespace UartTest
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
        public static byte[] sp_buffer;
        public static SerialPort Serial_Port;

        public MainWindow()
		{
			InitializeComponent();

            sp_buffer = new byte[10240]; // 10k memory

            cbPort.SelectionChanged += CbPort_SelectionChanged;
            cbPort.DisplayMemberPath = "PortInfo";

            RefreshCOMs();

            ManagementEventWatcher watcher = new ManagementEventWatcher();
            WqlEventQuery query = new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent WHERE (EventType = 2 OR EventType = 3)");
            watcher.EventArrived += Watcher_EventArrived;
            watcher.Query = query;
            watcher.Start();
        }

        void RefreshCOMs()
        {
            var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE Caption like '%(COM%'");
            var portnames = SerialPort.GetPortNames();
            var ports = searcher.Get().Cast<ManagementBaseObject>().ToList().Select(p => p["Caption"].ToString());
            var portList = portnames.Select(n => new { PortInfo = ports.FirstOrDefault(s => s.Contains(n)), Value = new SerialPort(n) });

            cbPort.ItemsSource = portList;
            cbPort.SelectedIndex = 0;
        }

        void Watcher_EventArrived(object sender, EventArrivedEventArgs e)
        {
            int evType = (int)(ushort)e.NewEvent.Properties["EventType"].Value;

            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background,
                (Action)delegate () { RefreshCOMs(); });
        }

        private void btnLoad_Click(object sender, RoutedEventArgs e)
		{
            // add more image formats at your requirements
			var images = GFL("jpg").Union(GFL("jpeg").Union(GFL("bmp").Union(GFL(("png")))));

			foreach (var p in images)
			{
				BitmapImage bimg = new BitmapImage();
				bimg.BeginInit();
				bimg.UriSource = new Uri(p, UriKind.Absolute);
				bimg.DecodePixelWidth = 200;
				bimg.CacheOption = BitmapCacheOption.OnLoad;
				bimg.EndInit();

				Image img = new Image();
				img.Source = bimg;
				img.Stretch = Stretch.Uniform;
				img.Height = 100;
                img.Tag = p;

				pnlItems.Children.Add(img);
				img.MouseEnter += delegate (object _sender, MouseEventArgs _e) { img.Opacity = .5; };
				img.MouseLeave += delegate (object _sender, MouseEventArgs _e) { img.Opacity = 1; };
                img.MouseUp += delegate (object _sender, MouseButtonEventArgs _e) { img.Opacity = 1; Task.Run(() => SendSerialDataToEsp(p)); };
            }
		}

        void SendSerialDataToEsp(string image_path )
        {
            var ms = new MemoryStream();
            var pic = new sd.Bitmap(image_path);
            var img = new sd.Bitmap(pic, new sd.Size(320, 240));

            var bmpData = img.LockBits(new sd.Rectangle(0, 0, img.Width, img.Height), sd.Imaging.ImageLockMode.ReadOnly, sd.Imaging.PixelFormat.Format16bppRgb565);
            byte[] bmpbytes = new byte[bmpData.Stride * img.Height];
            Marshal.Copy(bmpData.Scan0, bmpbytes, 0, bmpbytes.Length);
            img.UnlockBits(bmpData);

            // this is for tft byte-swapped format - maybe spi can be setup for that on esp
            for (int i = 0; i < bmpbytes.Length; i += 2)
            {
                byte msb = bmpbytes[i];
                byte lsb = bmpbytes[i + 1];
                bmpbytes[i] = lsb;
                bmpbytes[i + 1] = msb;
            }

            // port must be open to receive data
            if (!Serial_Port.IsOpen) Serial_Port.Open();
            Serial_Port.DataReceived += SerialPort_DataReceived;

            Serial_Port.Write(bmpbytes, 0, bmpbytes.Length);
        }

        string[] GFL(string extension) { return Directory.GetFiles(txtPath.Text, "*." + extension); }

        void CbPort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;

            Serial_Port = cbPort.SelectedItem.GetType().GetProperty("Value").GetValue(cbPort.SelectedItem) as SerialPort;

            Serial_Port.BaudRate = 115200;
            Serial_Port.DataBits = 8;
            Serial_Port.Parity = Parity.None;
            Serial_Port.StopBits = StopBits.One;
            Serial_Port.Handshake = Handshake.None;
        }

        void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // serial monitor is a simple textbox that most likely
            // does not display ANSI console output correctly
            // it is possible to improve that with RichTextBox

            int n = Serial_Port.Read(sp_buffer, 0, Serial_Port.BytesToRead);
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background,
                (Action)delegate ()
                {
                    txtSerMonit.Text += Encoding.ASCII.GetString(sp_buffer, 0, n);
                });
        }

        private void BtnBrowse_Click(object sender, RoutedEventArgs e)
        {
            // windows forms old-school dialog - can be improved
            using (System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog())
            {
                dlg.Description = "Select folder with pictures";
                dlg.SelectedPath = txtPath.Text;
                dlg.ShowNewFolderButton = false;
                System.Windows.Forms.DialogResult result = dlg.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    txtPath.Text = dlg.SelectedPath;
                    btnLoad_Click(sender, e);
                }
            }
        }
    }
}
