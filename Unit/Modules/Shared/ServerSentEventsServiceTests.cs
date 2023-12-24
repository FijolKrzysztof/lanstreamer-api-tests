using System.Collections.Concurrent;
using System.Reflection;
using System.Threading.Channels;
using lanstreamer_api.services;

namespace lanstreamer_api_tests.Unit.Modules.Shared;

public class ServerSentEventsServiceTests
{
    private readonly ServerSentEventsService<bool> _serverSentEventsService;
    
    public ServerSentEventsServiceTests()
    {
        _serverSentEventsService = new ServerSentEventsService<bool>();
    }
    
    //  public ChannelReader<T> Subscribe(string key)
    //    {
    //        var channel = Channel.CreateUnbounded<T>();
    //        _channels[key] = channel;
    //        return channel.Reader;
    //    }

    //    public async Task Unsubscribe(string key)
    //    {
    //        _channels.Remove(key, out Channel<T>? _);
    //    }

    //    public async Task Send(string key, T data)
    //    {
    //        if (_channels.TryGetValue(key, out var channel))
    //        {
    //            await channel.Writer.WriteAsync(data);
    //        }
    //    }
    
    [Fact]
    public void Subscribe_ShouldReturnChannelReader()
    {
        var channelReader = _serverSentEventsService.Subscribe("key");

        Assert.IsAssignableFrom<ChannelReader<bool>>(channelReader);
    }
    
    [Fact]
    public void Subscribe_ShouldAddChannel()
    {
        _serverSentEventsService.Subscribe("key");
        
        Assert.True(_serverSentEventsService
            .GetType()
            .GetField("_channels", BindingFlags.NonPublic | BindingFlags.Instance)!
            .GetValue(_serverSentEventsService) is ConcurrentDictionary<string, Channel<bool>> channels && channels.ContainsKey("key"));
    }
    
    [Fact]
    public void Unsubscribe_ShouldRemoveChannel()
    {
        _serverSentEventsService.Subscribe("key");
        _serverSentEventsService.Unsubscribe("key");
        
        Assert.False(_serverSentEventsService
            .GetType()
            .GetField("_channels", BindingFlags.NonPublic | BindingFlags.Instance)!
            .GetValue(_serverSentEventsService) is ConcurrentDictionary<string, Channel<bool>> channels && channels.ContainsKey("key"));
    }
    
    [Fact]
    public async Task Send_ShouldWriteToChannel()
    {
        var channelReader = _serverSentEventsService.Subscribe("key");
        await _serverSentEventsService.Send("key", true);
        
        Assert.True(channelReader.TryRead(out var result) && result);
    }
}