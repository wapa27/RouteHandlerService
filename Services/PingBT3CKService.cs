using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace RouteHandlerService.Services
{
    public class PingBT3CKService
    {
        private const int ListenPort = 5001; // The port to listen for acknowledgment

        public static async Task SendStartupPing()
        {
            byte[] ping = new byte[] { 0x52, 0x48, 0x50 }; // 'RHP'
            byte[] ping2 = new byte[] { 0x52, 0x48, 0x53 }; // 'RHS'
            using var client = new UdpClient();

            while (true)
            {
                try
                {
                    await client.SendAsync(ping, ping.Length, "127.0.0.1", 5000);
                    Console.WriteLine("Ping sent to BT3CKMsgHandler...");

                    var acknowledgmentReceived = await WaitForAcknowledgment(client);
                    if (acknowledgmentReceived)
                    {
                        Console.WriteLine("Acknowledgment received from BT3CKMsgHandler.");
                        _ = client.SendAsync(ping2, ping2.Length, "127.0.0.1", 5000);
                        break; // success, exit loop
                    }
                    else
                    {
                        Console.WriteLine("No acknowledgment received, retrying in 5 seconds...");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ping error: {ex.Message}");
                }

                Thread.Sleep(5000); // Blocking on purpose
            }
        }


        private static async Task<bool> WaitForAcknowledgment(UdpClient client)
        {
            try
            {
                var result = await client.ReceiveAsync(); // Wait for acknowledgment
                var data = result.Buffer;

                // Check if the acknowledgment matches expected data (e.g., "RHS" as acknowledgment)
                if (data.Length == 3 && data[0] == 0x52 && data[1] == 0x48 && data[2] == 0x53)
                {
                    return true; // Acknowledgment received
                }
                else
                {
                    Console.WriteLine("Not expected format");
                    return false; // Unexpected data
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error receiving acknowledgment: {ex.Message}");
                return false; // If there's an error or timeout, return false
            }
        }
    }
}
