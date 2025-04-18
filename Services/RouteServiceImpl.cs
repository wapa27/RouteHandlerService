using Grpc.Core;
using RouteHandler;
using SignsAndAudioService;
using System.Text;
using static SignsAndAudioService.SignsAndAudioService;

namespace RouteHandlerService.Services
{
    public class RouteServiceImpl: RouteHandler.RouteService.RouteServiceBase
    {
        private readonly SignsAndAudioServiceClient _client;
        private readonly string _baseDataPath = Path.Combine(
            Directory.GetCurrentDirectory(), "Data"
        );
        public RouteServiceImpl(SignsAndAudioServiceClient client) {
            _client = client;
        }
        public override Task<RouteResponse> SendRoute(RouteRequest request, ServerCallContext context)
        {
            Console.WriteLine("RouteHandlerService Reached");
            Console.WriteLine(request.RouteId);

            _ = SendToSAASAsync(request.RouteId);

            return Task.FromResult(new RouteResponse { Success = true });
        }

        public async Task SendToSAASAsync(int index)
        {
            try
            {
                var messages = new[]
                {
            GetMessageFromFile("InsideSigns.csv", index),
            GetMessageFromFile("OutsideSigns.csv", index),
            GetMessageFromFile("InsideSpeakers.csv", index),
            GetMessageFromFile("OutsideSpeakers.csv", index)
        };

                var data = Encoding.UTF8.GetBytes(string.Join(";", messages));
                var request = new MessageRequest { Data = Google.Protobuf.ByteString.CopyFrom(data) };

                var reply = await _client.ReceiveMessagesAsync(request);
                Console.WriteLine($"SAAS response: {reply.Status}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SendToSAASAsync failed: {ex.Message}");
            }
        }

        private string GetMessageFromFile(string filename, int index)
        {
            try
            {
                var path = Path.Combine(_baseDataPath, filename);
                foreach (var line in File.ReadLines(path))
                {
                    var parts = line.Split(',', 2);
                    if (parts.Length == 2 && int.TryParse(parts[0], out int lineIndex) && lineIndex == index)
                        return parts[1];
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading {filename}: {ex.Message}");
            }

            return "";
        }

    }

}
