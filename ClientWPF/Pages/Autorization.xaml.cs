using System;
using System.Collections.Generic;
using System.Net;
using Common;
using System.Net.Sockets;
using System.Text;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json;

namespace ClientWPF.Pages
{
    /// <summary>
    /// Логика взаимодействия для Autorization.xaml
    /// </summary>
    public partial class Autorization : Page
    {
        public Autorization()
        {
            InitializeComponent();
        }

        private void ClickConnection(object sender, RoutedEventArgs e)
        {
            if (!IPAddress.TryParse(IP.Text, out MainWindow.mainWindow.IpAddress))
            {
                MessageBox.Show("Указан не правильный ip адрес", "Ошибка");
                return;
            }


            if (!int.TryParse(Port.Text, out MainWindow.mainWindow.Port))
            {
                MessageBox.Show("Указан не правильный port адрес", "Ошибка");
                return;
            }

            var socket = MainWindow.mainWindow.ConnectToServer();

            if (socket == null || !ConnectToServer(socket, Login.Text, Password.Text))
            {
                MessageBox.Show("Не удалось подключиться", "Ошибка");
            }
            else
            {
                MainWindow.mainWindow.OpenPages(new Pages.Main());
            }
        }
        public bool ConnectToServer(Socket socket, string login, string password)
        {
            try
            {
                string command = $"connect {login} {password}";
                var viewModelSend = new ViewModelSend(command, -1);
                byte[] messageBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(viewModelSend));
                socket.Send(messageBytes);
                byte[] buffer = new byte[1024];
                int bytesReceived = socket.Receive(buffer);
                string response = Encoding.UTF8.GetString(buffer, 0, bytesReceived);
                ViewModelMessage viewModelMessage = JsonConvert.DeserializeObject<ViewModelMessage>(response);
                socket.Close();
                if (viewModelMessage.Command == "autorization")
                {
                    int userId = int.Parse(viewModelMessage.Data);
                    MainWindow.mainWindow.Id = userId;
                    MessageBox.Show($"Успешная авторизация. Ваш ID: {userId}", "Успех");
                    return true;
                }
                else
                {
                    MessageBox.Show($"Ошибка авторизации: {viewModelMessage.Data}", "Ошибка");
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при подключении: {ex.Message}", "Ошибка");
                return false;
            }
        }
    }
}
