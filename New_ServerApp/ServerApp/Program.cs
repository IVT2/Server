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
        const int port = 8888;
        static TcpListener listener;
        static void Main(string[] args)
        {           

            try
            {
                listener = new TcpListener(IPAddress.Parse("192.168.0.115"), port);
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

    class Person
    {
        public string login { get; set; }
        public string password { get; set; }
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
                byte[] passwordData = new byte[64];
                byte[] userData = new byte[64];
                byte[] data = new byte[64];

                // получаем сообщение
                StringBuilder builder = new StringBuilder();
                StringBuilder source = new StringBuilder();
                int passBytes = 0;
                int UserBytes = 0;
                do
                {
                    UserBytes = stream.Read(userData, 0, userData.Length);
                    source.Append(Encoding.Unicode.GetString(userData, 0, UserBytes));
                    passBytes = stream.Read(passwordData, 0, passwordData.Length);
                    builder.Append(Encoding.Unicode.GetString(passwordData, 0, passBytes));
                    
                }
                while (stream.DataAvailable); 

                    

                string pass = builder.ToString();
                string login = source.ToString();

                string allertmasseg = "User dosnt exist";

                List<Person> people = new List<Person>();
                people.Add(new Person() { login = "ivt22222", password = "ivt2121" });

                foreach (Person p in people)
                {
                    if (p.login == login & p.password == pass)
                    {
                        Console.WriteLine("Ok...");
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Not Found");
                        data = Encoding.Unicode.GetBytes(allertmasseg);
                        stream.Write(data, 0, data.Length);
                    }
                }

                Console.WriteLine(login);

                Console.WriteLine(pass);
              
                
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
