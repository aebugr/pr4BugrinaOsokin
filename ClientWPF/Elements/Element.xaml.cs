using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ClientWPF.Pages;
using Common;
using Newtonsoft.Json;

namespace ClientWPF.Elements
{
    /// <summary>
    /// Логика взаимодействия для Element.xaml
    /// </summary>
    public partial class Element : UserControl
    {
        string Path;
        bool Server = false;
        bool Fileb = false;

        Main Main;
        public Element(string path, string name, bool server, Main Main)
        {
            InitializeComponent();
            this.Path = path;
            name = name.Replace("\\", "");
            if (path[path.Length - 1] == '\\')
            {
                FileIcon.Visibility = Visibility.Hidden;
            }
            else
            {
                FolderIcon.Visibility = Visibility.Hidden;
                Fileb = true;
            }
            NameElement.Content = name;
            this.Server = server;
            this.Main = Main;
        }

        private void OpenOrSend_Click(object sender, MouseButtonEventArgs e)
        {
            if (Server)
            {
                if (Fileb)
                {
                    Main.DownloadFileFromServer(Path);
                }
                else
                {
                    Main.OpenDirectoryServer(NameElement.ToString());
                }
            }
            else
            {
                if (Fileb)
                {
                    SendFileToServer(Path);
                }
                else
                {
                    Main.OpenDirectoryClient(Path);
                }
            }
        }
        private void SendFileToServer(string filePath)
        {
            try
            {
                var socket = MainWindow.mainWindow.ConnectToServer();
                var userId = MainWindow.mainWindow.Id;
                if (socket == null)
                {
                    MessageBox.Show("Не удалось подключиться к серверу.", "Ошибка подключения");
                    return;
                }
                if (!File.Exists(filePath))
                {
                    MessageBox.Show("Указанный файл не существует.", "Ошибка");
                    return;
                }
                FileInfo fileInfo = new FileInfo(filePath);
                FileInfoFTP fileInfoFTP = new FileInfoFTP(File.ReadAllBytes(filePath), fileInfo.Name);
                ViewModelSend viewModelSend = new ViewModelSend(JsonConvert.SerializeObject(fileInfoFTP), userId);
                byte[] messageByte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(viewModelSend));
                socket.Send(messageByte);
                byte[] buffer = new byte[10485760];
                int bytesReceived = socket.Receive(buffer);
                string serverResponse = Encoding.UTF8.GetString(buffer, 0, bytesReceived);
                ViewModelMessage responseMessage = JsonConvert.DeserializeObject<ViewModelMessage>(serverResponse);
                socket.Close();
                Main.UpdateDir();
                if (responseMessage.Command == "message")
                {
                    MessageBox.Show(responseMessage.Data, "Ответ сервера");
                }
                else
                {
                    MessageBox.Show("Неизвестный ответ от сервера.", "Ошибка");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при отправке файла: {ex.Message}", "Ошибка");
            }
        }
    }
}
