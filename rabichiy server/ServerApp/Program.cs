using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Collections.Generic;

namespace ConsoleServer
{
    
    class Program
    {

        class Person
        {
            public string login { get; set; }
            public string password { get; set; }
        }

        const int port = 8888;
        static TcpListener listener;
        static void Main(string[] args)
        {

            Dictionary<int, Person> people = new Dictionary<int, Person>();
            people.Add(1, new Person() { login = "Bill" , password = "1234"});
            people.Add(2, new Person() { login = "Bill2", password = "5678" });
            people.Add(3, new Person() { login = "Bill3", password = "4321" });

            foreach (KeyValuePair<int, Person> keyValue in people)
            {
                Console.WriteLine(keyValue.Value.login + " - " + keyValue.Value.password);
                if (keyValue.Value.login == "Bill2" )
                {
                    Console.WriteLine("Value : India is present.");
                }
                    
            }

            try
            {
                listener = new TcpListener(IPAddress.Parse("192.168.0.111"), port);
                listener.Start();
                Console.WriteLine("Ожидание подключений...");

                while (true)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    ClientObject clientObject = new ClientObject(client);

                    // создаем новый поток для обслуживания нового клиента
                    Thread clientThread = new Thread(new ThreadStart(clientObject.Process));
                    clientThread.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (listener != null)
                    listener.Stop();
            }
        }
    }
    public class ClientObject
    {
        public TcpClient client;
        public ClientObject(TcpClient tcpClient)
        {
            client = tcpClient;
        }

        public void Process()
        {
            NetworkStream stream = null;
            try
            {
                stream = client.GetStream();
                byte[] data = new byte[64]; // буфер для получаемых данных
                byte[] userData = new byte[64];
                int i = 0;
                while (i<1)
                {
                    // получаем сообщение
                    StringBuilder builder = new StringBuilder();
                    StringBuilder source = new StringBuilder();
                    int bytes = 0;
                    int UserBytes = 0;
                    do
                    {
                        bytes = stream.Read(data, 0, data.Length);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                        UserBytes = stream.Read(userData, 0, userData.Length);
                        source.Append(Encoding.Unicode.GetString(userData, 0, UserBytes));
                    }
                    while (stream.DataAvailable);

                    

                    string message = builder.ToString();
                    string src = source.ToString();
                    Console.WriteLine(message);
                    Console.WriteLine(src);

                    


                    // отправляем обратно сообщение в верхнем регистре
                    message = message.Substring(message.IndexOf(':') + 1).Trim().ToUpper();
                    data = Encoding.Unicode.GetBytes(message);
                    stream.Write(data, 0, data.Length);
                    i++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (stream != null)
                    stream.Close();
                if (client != null)
                    client.Close();
            }
        }
    }
}
