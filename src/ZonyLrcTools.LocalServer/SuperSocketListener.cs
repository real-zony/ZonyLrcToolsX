using Newtonsoft.Json;
using SuperSocket.WebSocket.Server;

namespace ZonyLrcTools.LocalServer;

public class SuperSocketListener
{
    public async Task ListenAsync()
    {
        var host = WebSocketHostBuilder.Create()
            .UseWebSocketMessageHandler(async (session, message) =>
            {
                await session.SendAsync(JsonConvert.SerializeObject(new CommunicateEvent<OutputEvent>
                {
                    Action = "output",
                    Type = ActionType.ResponseAction,
                    Data = new OutputEvent()
                }));

                for (int index = 0; index < 50; index++)
                {
                    var data = new List<GetFileInfoEvent>();
                    for (int j = 0; j < 500; j++)
                    {
                        data.Add(new GetFileInfoEvent
                        {
                            Name = j.ToString(),
                            Size = j.ToString()
                        });
                    }

                    await session.SendAsync(JsonConvert.SerializeObject(new CommunicateEvent<List<GetFileInfoEvent>>
                    {
                        Action = "getFileInfo",
                        Type = ActionType.ResponseAction,
                        Data = data
                    }));
                }

                await Task.Delay(100);
            })
            .Build();

        await host.StartAsync();
    }
}

public class CommunicateEvent<T> where T : class
{
    [JsonProperty("action")] public string? Action { get; set; }

    [JsonProperty("type")] public ActionType Type { get; set; }

    [JsonProperty("data")] public T? Data { get; set; }
}

public class OutputEvent
{
    [JsonProperty("text")] public string Text => DateTime.Now.ToString("HH:mm:ss");
}

public class GetFileInfoEvent
{
    [JsonProperty("name")] public string Name { get; set; }
    [JsonProperty("size")] public string Size { get; set; }
}

public enum ActionType
{
    RequestAction,
    ResponseAction
}