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

                Console.WriteLine("Server is running and waiting for connections from Client1...");

                //////////////////////////////////////CLIENT2///////////////////////////////////////////

                IPEndPoint client2EndPoint = new IPEndPoint(IPAddress.Parse(serverIP), client2Port);
                using Socket client2Listener = new Socket(client2EndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                // Bind and listen for connections from client2
                client2Listener.Bind(client2EndPoint);
                client2Listener.Listen(100);

                Console.WriteLine("Server is running and waiting for connections from Client2...");

                ///////////////////////////////////////////////////////////////////////////////////

                while (true)
                {

                    ///////////////////////////////////CLIENT1////////////////////////////////////////////
                    Socket client1Handler = await client1Listener.AcceptAsync();

                    byte[] buffer = new byte[1024];
                    int received = await client1Handler.ReceiveAsync(buffer, SocketFlags.None);
                    string message = Encoding.UTF8.GetString(buffer, 0, received);

                    Console.WriteLine("Received message from client1: " + message);


                    string[] parts = message.Split(',');

                    Console.WriteLine($"n: {parts[0]}");
                    Console.WriteLine($"e: {parts[1]}");
                    Console.WriteLine($"d: {parts[2]}");
                    Console.WriteLine($"x: {parts[3]}");
                    Console.WriteLine($"s: {parts[4]}");

                    WriteToFile(publicKeyFile, $"{parts[0]}\n{parts[1]}");
                    WriteToFile(privateKeyFile, $"{parts[2]}");
                    WriteToFile(signatureFile, $"{parts[3]}\n{parts[4]}");

    


                    client1Handler.Shutdown(SocketShutdown.Both);
                    client1Handler.Close();

                    /////////////////////////////////////CLIENT2//////////////////////////////////////////////

                    Socket client2Handler = await client2Listener.AcceptAsync();

                    Console.Write("Ar norite pakeisti skaitmeninio paraso reiksme (T/N): ");
                    string pasirinkimas = Console.ReadLine();

                    switch (pasirinkimas)
                    {
                        case "T":
                            Console.WriteLine($"x: {parts[3]}");
                            Console.WriteLine($"s: {parts[4]}");

                            Console.Write($"x: ");
                            string newX = Console.ReadLine();
                            Console.Write($"s: ");
                            string newS = Console.ReadLine();

                            message = $"{parts[0]},{parts[1]},{parts[2]},{newX},{newS}";
                            Console.WriteLine($"Zinute siunciama clientui2: {message}");
                            SendMessageToClient2(serverIP, client2Port, message);

                            break;

                        case "N":
                            Console.WriteLine($"Zinute siunciama clientui2: {message}");
                            SendMessageToClient2(serverIP, client2Port, message);
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

        private static void SendMessageToClient2(string client2IP, int client2Port, string message)
        {
            try
            {
                using Socket client2Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                client2Socket.Connect(client2IP, client2Port);

                string[] parts = message.Split(',');

                byte[] messageBytes = Encoding.UTF8.GetBytes($"{parts[0]},{parts[1]},{parts[2]},{parts[3]},{parts[4]}");

                client2Socket.Send(messageBytes);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send message to Client2: {ex.Message}");
            }
        }

        private static string publicKeyFile = "C:\\Users\\pugli\\OneDrive\\Stalinis kompiuteris\\Desktop\\Kolegija\\4 semestras\\INFORMACIJOS SAUGUMAS\\Server\\publicKey.txt";
        private static string privateKeyFile = "C:\\Users\\pugli\\OneDrive\\Stalinis kompiuteris\\Desktop\\Kolegija\\4 semestras\\INFORMACIJOS SAUGUMAS\\Server\\privateKey.txt";
        private static string signatureFile = "C:\\Users\\pugli\\OneDrive\\Stalinis kompiuteris\\Desktop\\Kolegija\\4 semestras\\INFORMACIJOS SAUGUMAS\\Server\\signature.txt";

        private static void WriteToFile(string fileName, string text)
        {
            using (StreamWriter writer = new StreamWriter(fileName))
            {
                writer.Write(text);
            }
        }

        public static bool ValidateSignature(string[] parts)
        {
            BigInteger n = BigInteger.Parse(parts[0]);
            BigInteger e = BigInteger.Parse(parts[1]);
            string[] xParts = parts[3].Split(' ');
            string[] sParts = parts[4].Split(' ');

            BigInteger[] xBigIntegers = new BigInteger[xParts.Length];
            BigInteger[] sBigIntegers = new BigInteger[sParts.Length];

            for (int i = 0; i < xParts.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(xParts[i]))
                {
                    continue;
                }

                if (!BigInteger.TryParse(xParts[i], out xBigIntegers[i]))
                {
                    Console.WriteLine($"Error: Failed to parse '{xParts[i]}' into a BigInteger.");
                    return false;
                }
            }

            for (int i = 0; i < sParts.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(sParts[i]))
                {
                    continue;
                }

                if (!BigInteger.TryParse(sParts[i], out sBigIntegers[i]))
                {
                    Console.WriteLine($"Error: Failed to parse '{sParts[i]}' into a BigInteger.");
                    return false;
                }
            }

            for (int i = 0; i < xBigIntegers.Length; i++)
            {
                BigInteger sPowE = BigInteger.ModPow(sBigIntegers[i], e, n);

                if (!sPowE.Equals(xBigIntegers[i]))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
