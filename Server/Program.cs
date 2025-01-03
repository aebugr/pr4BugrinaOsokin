﻿using Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    internal class Program
    {
        public static List<User> Users = new List<User>();
        public static IPAddress IpAddress;
        public static int Port;
        public static bool AutorizationUser(string login, string password)
        {
            User user = null;
            user = Users.Find(x => x.login == login && x.password == password);
            return user != null;
        }
        public static List<string> GetDirectory(string src)
        {
            List<string> FoldersFiles = new List<string>();
            if (Directory.Exists(src))
            {
                string[] dirs = Directory.GetDirectories(src);
                foreach (string dir in dirs)
                {
                    string NameDirectory = dir.Replace(src, "");
                    FoldersFiles.Add(NameDirectory + "\\");
                }
                string[] files = Directory.GetFiles(src);
                foreach (string file in files)
                {
                    string NameFile = file.Replace(src, "");
                    FoldersFiles.Add($"{NameFile}");
                }
            }
            return FoldersFiles;
        }
        public static void StartServer()
        {
            IPEndPoint endPoint = new IPEndPoint(IpAddress, Port);
            Socket sListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            sListener.Bind(endPoint);
            sListener.Listen(10);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Сервер запущен.");
            while (true)
            {
                try
                {
                    Socket Handler = sListener.Accept();
                    string Data = null;
                    byte[] Bytes = new byte[10485760];
                    int BytesRec = Handler.Receive(Bytes);
                    Data += Encoding.UTF8.GetString(Bytes, 0, BytesRec);
                    Console.Write("Сообщения от пользователя: " + Data + "\n");
                    string Reply = "";
                    ViewModelSend ViewModelSend = JsonConvert.DeserializeObject<ViewModelSend>(Data);
                    if (ViewModelSend != null)
                    {
                        ViewModelMessage viewModelMessage;
                        string[] DataCommand = ViewModelSend.Message.Split(new string[1] { " " }, StringSplitOptions.None);
                        if (DataCommand[0] == "connect")
                        {
                            string[] DataMessage = ViewModelSend.Message.Split(new string[1] { " " }, StringSplitOptions.None);
                            if (AutorizationUser(DataMessage[1], DataMessage[2]))
                            {
                                int IdUser = Users.FindIndex(x => x.login == DataMessage[1] && x.password == DataMessage[2]);
                                viewModelMessage = new ViewModelMessage("autorization", IdUser.ToString());
                                Database.AddUserCommand(Users[IdUser].id, ViewModelSend.Message);
                            }
                            else
                            {
                                viewModelMessage = new ViewModelMessage("message", "Неправильный логин и пароль пользователя.");
                            }
                            Reply = JsonConvert.SerializeObject(viewModelMessage);
                            byte[] message = Encoding.UTF8.GetBytes(Reply);
                            Handler.Send(message);
                        }
                        else if (DataCommand[0] == "cd")
                        {
                            if (ViewModelSend.Id != -1)
                            {
                                string[] DataMessage = ViewModelSend.Message.Split(new string[1] { " " }, StringSplitOptions.None);
                                List<string> FoldersFiles = new List<string>();
                                if (DataMessage.Length == 1)
                                {
                                    Users[ViewModelSend.Id].temp_src = Users[ViewModelSend.Id].src;
                                    FoldersFiles = GetDirectory(Users[ViewModelSend.Id].src);
                                }
                                else
                                {
                                    string cdFolder = "";
                                    for (int i = 1; i < DataMessage.Length; i++)
                                    {
                                        cdFolder += (i > 1 ? " " : "") + DataMessage[i];
                                    }
                                    cdFolder = cdFolder.Trim('"');
                                    string fullPath = Path.Combine(Users[ViewModelSend.Id].temp_src, cdFolder);
                                    Console.WriteLine($"Переход в директорию: {fullPath}");
                                    if (!Directory.Exists(fullPath))
                                    {
                                        Console.WriteLine("Директория не существует.");
                                        viewModelMessage = new ViewModelMessage("message", "Директория пуста или не существует.");
                                        Reply = JsonConvert.SerializeObject(viewModelMessage);
                                        Handler.Send(Encoding.UTF8.GetBytes(Reply));
                                        return;
                                    }

                                    Users[ViewModelSend.Id].temp_src = fullPath;
                                    FoldersFiles = GetDirectory(fullPath);
                                }

                                if (FoldersFiles.Count == 0)
                                {
                                    viewModelMessage = new ViewModelMessage("message", "Директория пуста или не существует.");
                                }
                                else
                                {
                                    viewModelMessage = new ViewModelMessage("cd", JsonConvert.SerializeObject(FoldersFiles));
                                }
                            }
                            else
                            {
                                viewModelMessage = new ViewModelMessage("message", "Необходимо авторизоваться.");
                            }
                            Reply = JsonConvert.SerializeObject(viewModelMessage);
                            Handler.Send(Encoding.UTF8.GetBytes(Reply));
                        }
                        else if (DataCommand[0] == "get")
                        {
                            if (ViewModelSend.Id != -1)
                            {
                                string getFile = "";
                                string[] DataMessage = ViewModelSend.Message.Split(new string[1] { " " }, StringSplitOptions.None);
                                for (int i = 1; i < DataMessage.Length; i++)
                                {
                                    getFile += (i > 1 ? " " : "") + DataMessage[i];
                                }
                                string fullPath = Path.Combine(Users[ViewModelSend.Id].temp_src, getFile.TrimStart('\\'));
                                Console.WriteLine($"Запрашиваемый файл: {getFile}");
                                Console.WriteLine($"Полный путь к файлу на сервере: {fullPath}");
                                Console.WriteLine($"Текущая директория пользователя: {Users[ViewModelSend.Id].temp_src}");
                                if (!File.Exists(fullPath))
                                {
                                    Console.WriteLine("Файл не найден.");
                                    viewModelMessage = new ViewModelMessage("message", "Файл не найден.");
                                    return;
                                }
                                else
                                {
                                    try
                                    {
                                        byte[] byteFile = File.ReadAllBytes(fullPath);
                                        viewModelMessage = new ViewModelMessage("file", JsonConvert.SerializeObject(byteFile));
                                        Console.WriteLine($"Файл успешно прочитан: {getFile}");
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"Ошибка чтения файла: {ex.Message}");
                                        viewModelMessage = new ViewModelMessage("message", "Ошибка при чтении файла.");
                                    }
                                }
                                Reply = JsonConvert.SerializeObject(viewModelMessage);
                                byte[] message = Encoding.UTF8.GetBytes(Reply);
                                Handler.Send(message);
                            }
                            else
                            {
                                viewModelMessage = new ViewModelMessage("message", "Необходимо авторизоваться.");
                            }
                        }
                        else
                        {
                            if (ViewModelSend.Id != -1)
                            {
                                FileInfoFTP SendFileInfo = JsonConvert.DeserializeObject<FileInfoFTP>(ViewModelSend.Message);
                                File.WriteAllBytes(Users[ViewModelSend.Id].temp_src + @"\" + SendFileInfo.Name, SendFileInfo.Data);
                                viewModelMessage = new ViewModelMessage("message", "Файл загружен");
                                Database.AddUserCommand(Users[ViewModelSend.Id].id, ViewModelSend.Message);
                            }
                            else
                            {
                                viewModelMessage = new ViewModelMessage("message", "Необходимо авторизоваться.");
                            }
                            Reply = JsonConvert.SerializeObject(viewModelMessage);
                            byte[] message = Encoding.UTF8.GetBytes(Reply);
                            Handler.Send(message);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Исключение: " + e.Message);
                }
            }
        }
        static void Main(string[] args)
        {
            Users = Database.GetUsers();
            Console.Write("Введите IP адрес сервер: ");
            string sIpAddress = Console.ReadLine();
            Console.Write("Введите порт: ");
            string sPort = Console.ReadLine();
            if (int.TryParse(sPort, out Port) && IPAddress.TryParse(sIpAddress, out IpAddress))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Данные успешнно введены. Сервер запускается.");
                StartServer();
            }
            Console.Read();
        }
    }
}
