using SuperSocket.WebSocket.Server;

namespace ZonyLrcTools.LocalServer.EventBus;

public class SuperSocketListener
{
    public async Task ListenAsync()
    {
        var host = WebSocketHostBuilder.Create()
            .UseWebSocketMessageHandler(async (session, message) => { await session.SendAsync(message); })
            .Build();

        await host.StartAsync();
    }
}