using lanstreamer_api.Models;
using Newtonsoft.Json;

namespace lanstreamer_api_tests.Unit;

public class UserDtoTests
{
    [Fact]
    public void ShouldCorrectlySerialize()
    {
        var clientDto = new UserDto()
        {
            Id = 1,
            AccessCode = "1234",
        };

        var serializedClient = JsonConvert.SerializeObject(clientDto);
        
        Assert.Contains("id", serializedClient);
        Assert.Contains("accessCode", serializedClient);
    }
    
    [Fact]
    public void ShouldCorrectlyDeserialize()
    {
        var serializedClient = "{\"id\":1,\"accessCode\":\"1234\"}";
        var deserializedClient = JsonConvert.DeserializeObject<UserDto>(serializedClient);
        
        Assert.Equal(1, deserializedClient.Id);
        Assert.Equal("1234", deserializedClient.AccessCode);
    }
}