using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Collections.Generic;
using InstaSharper.API;
using InstaSharper.API.Builder;
using InstaSharper.API.Processors;
using InstaSharper.API.UriCreators;
using InstaSharper.Classes;
using InstaSharper.Converters;
using InstaSharper.Helpers;
using InstaSharper.Logger;
using InstaSharper.Classes.Models;
using System.Threading.Tasks;
using System.Linq;

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

                

                Console.WriteLine("Wauting conntecting...");

                

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

        private static UserSessionData user;
        private static IInstaApi api;


        public async void Process()
        {
            NetworkStream stream = null;
            try
            {
                stream = client.GetStream();
                byte[] passwordData = new byte[64];
                byte[] userData = new byte[64];
                byte[] data = new byte[64];

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

                Console.WriteLine(login);

                Console.WriteLine(pass);

                user = new UserSessionData();
                user.UserName = login;
                user.Password = pass;

                string allertmasseg = "User dosnt exist";
                string findmasseg = "Existing user";

                api = InstaApiBuilder.CreateBuilder()
                    .SetUser(user)
                    .UseLogger(new DebugLogger(LogLevel.Exceptions))
                    .Build();

                var loginRequest = await api.LoginAsync();
                if (loginRequest.Succeeded)
                {
                    Console.WriteLine("Logged in!");
                    data = Encoding.Unicode.GetBytes(findmasseg);
                    stream.Write(data, 0, data.Length);
                    PullUserPosts("mihasquid");
                }
                else
                {
                    Console.WriteLine("Error logging in!" + loginRequest.Info.Message);
                    data = Encoding.Unicode.GetBytes(allertmasseg);
                    stream.Write(data, 0, data.Length);
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

        public static async void PullUserPosts(string userToScrape)
        {
            Console.WriteLine("Wait");
            IResult<InstaUser> userSearch = await api.GetUserAsync(userToScrape);
            Console.WriteLine(userSearch);
            Console.WriteLine("done");
        }
    }
}
