using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                string serverIP = "127.0.0.1";

                int client1Port = 7070;
                int client2Port = 7171;

                /////////////////////////////////////CLIENT1//////////////////////////////////////////////

                IPEndPoint client1EndPoint = new IPEndPoint(IPAddress.Parse(serverIP), client1Port);
                using Socket client1Listener = new Socket(client1EndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                client1Listener.Bind(client1EndPoint);
                client1Listener.Listen(100);

                Console.WriteLine("Waiting for connections from Client1...");

                //////////////////////////////////////CLIENT2///////////////////////////////////////////

                IPEndPoint client2EndPoint = new IPEndPoint(IPAddress.Parse(serverIP), client2Port);
                using Socket client2Listener = new Socket(client2EndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                // Bind and listen for connections from client2
                client2Listener.Bind(client2EndPoint);
                client2Listener.Listen(100);

                Console.WriteLine("Waiting for connections from Client2...");

                ///////////////////////////////////////////////////////////////////////////////////

                while (true)
                {

                    ///////////////////////////////////CLIENT1////////////////////////////////////////////
                    Socket client1Handler = await client1Listener.AcceptAsync();

                    byte[] buffer = new byte[1024];
                    int received = await client1Handler.ReceiveAsync(buffer, SocketFlags.None);
                    string message = Encoding.UTF8.GetString(buffer, 0, received);

                    string[] parts = message.Split(',');

                    Console.WriteLine($"n: {parts[0]}");
                    Console.WriteLine($"e: {parts[1]}");
                    Console.WriteLine($"x: {parts[2]}");
                    Console.WriteLine($"s: {parts[3]}");

                    client1Handler.Shutdown(SocketShutdown.Both);
                    client1Handler.Close();

                    /////////////////////////////////////CLIENT2//////////////////////////////////////////////

                    Socket client2Handler = await client2Listener.AcceptAsync();

                    Console.Write("Ar norite pakeisti skaitmeninio paraso reiksme (T/N): ");
                    string pasirinkimas = Console.ReadLine();

                    byte[] messageBytes;

                    switch (pasirinkimas)
                    {
                        case "T":
                            Console.WriteLine($"x: {parts[2]}");
                            Console.WriteLine($"s: {parts[3]}");

                            Console.Write($"x: ");
                            string newX = Console.ReadLine();
                            Console.Write($"s: ");
                            string newS = Console.ReadLine();

                            message = $"{parts[0]},{parts[1]},{newX},{newS}";

                            messageBytes = Encoding.UTF8.GetBytes(message);
                            await client2Handler.SendAsync(messageBytes, SocketFlags.None);
                            break;

                        case "N":
                            messageBytes = Encoding.UTF8.GetBytes(message);
                            await client2Handler.SendAsync(messageBytes, SocketFlags.None);
                            break;

                        default:
                            Console.WriteLine("Tokio pasirinkimo nera!");
                            break;
                    }

                    client2Handler.Shutdown(SocketShutdown.Both);
                    client2Handler.Close();
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }
}
