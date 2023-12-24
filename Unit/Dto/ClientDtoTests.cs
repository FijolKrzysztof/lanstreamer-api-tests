using lanstreamer_api.Models;
using Newtonsoft.Json;

namespace lanstreamer_api_tests.Unit;

public class ClientDtoTests
{
    [Fact]
    public void ShouldCorrectlySerialize()
    {
        var clientDto = new ClientDto
        {
            Id = 1,
            ReferrerWebsite = "www.example.com",
        };

        var serializedClient = JsonConvert.SerializeObject(clientDto);
        
        Assert.Contains("id", serializedClient);
        Assert.Contains("referrerWebsite", serializedClient);
    }
    
    [Fact]
    public void ShouldCorrectlyDeserialize()
    {
        var serializedClient = "{\"id\":1,\"referrerWebsite\":\"www.example.com\"}";
        var deserializedClient = JsonConvert.DeserializeObject<ClientDto>(serializedClient);
        
        Assert.Equal(1, deserializedClient.Id);
        Assert.Equal("www.example.com", deserializedClient.ReferrerWebsite);
    }
}