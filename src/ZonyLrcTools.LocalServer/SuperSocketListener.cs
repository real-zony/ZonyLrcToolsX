using SuperSocket.WebSocket.Server;

namespace ZonyLrcTools.LocalServer;

public class SuperSocketListener
{
    public async Task ListenAsync()
    {
        var host = WebSocketHostBuilder.Create()
            .UseWebSocketMessageHandler(async (session, message) => { await session.SendAsync(message.Message); })
            .Build();

        await host.StartAsync();
    }
}