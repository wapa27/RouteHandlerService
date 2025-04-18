using RouteHandlerService.Services;
using SignsAndAudioService;
using static SignsAndAudioService.SignsAndAudioService;

var builder = WebApplication.CreateBuilder(args);

try
{
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenLocalhost(5102, o => o.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2);
    });

    builder.Services.AddGrpc();

    builder.Services.AddGrpcClient<SignsAndAudioServiceClient>(options =>
    {
        options.Address = new Uri("http://localhost:5103");
    });

    var app = builder.Build();

    try
    {
        await PingBT3CKService.SendStartupPing();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"PingBT3CKService failed: {ex.Message}");
    }

    app.MapGrpcService<RouteServiceImpl>();
    app.MapGet("/", () => "This server uses gRPC. Use a gRPC client to communicate.");

    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"Fatal startup error: {ex.Message}");
}
