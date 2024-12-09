using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ClientWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow mainWindow;
        public IPAddress IpAddress;
        public int Port;
        public int Id = -1;
        public MainWindow()
        {
            InitializeComponent();
            mainWindow = this;
            OpenPages(new Pages.Autorization());
        }
        public Socket ConnectToServer()
        {
            IPEndPoint endPoint = new IPEndPoint(IpAddress, Port);
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                socket.Connect(endPoint);
                if (socket.Connected)
                {
                    return socket;
                }
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex.Message}", "Ошибка");
                return null;
            }
        }

        public void OpenPages(Page page)
        {
            frame.Navigate(page);
        }
    }
}
