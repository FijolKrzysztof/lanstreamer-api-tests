using System.Net.Http.Headers;
using System.Text;
using lanstreamer_api_tests.Integration.Abstract;
using lanstreamer_api.App.Data.Dto.Responses;
using lanstreamer_api.App.Data.Models.Enums;
using lanstreamer_api.Data.Configuration;
using lanstreamer_api.Data.Modules.AccessCode;
using lanstreamer_api.Models;
using lanstreamer_api.Models.Responses;
using Newtonsoft.Json;

namespace lanstreamer_api_tests.Integration;

public class UserControllerTests : IntegrationTestsBase
{
    [Fact]
    public async Task Login_ShouldNotAuthorize_WhenNoGoogleId()
    {
        var response = await Client.PostAsync("/api/user/login", new StringContent(
            JsonConvert.SerializeObject(new UserDto()),
            Encoding.UTF8,
            "application/json"
        ));

        var responseContent = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(responseContent)!;

        Assert.Equal(401, (int)response.StatusCode);
        Assert.Equal("Missing google token", errorResponse.Message);
        Assert.Equal(401, errorResponse.StatusCode);
    }

    [Fact]
    public async Task Login_ShouldNotAuthorize_WhenWrongGoogleId()
    {
        const string accessToken = "incorrect-token";

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await Client.PostAsync("/api/user/login", new StringContent(
            JsonConvert.SerializeObject(new UserDto()),
            Encoding.UTF8,
            "application/json"
        ));

        var responseContent = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(responseContent)!;

        Assert.Equal(401, (int)response.StatusCode);
        Assert.Equal("Invalid google token", errorResponse.Message);
        Assert.Equal(401, errorResponse.StatusCode);
    }

    [Fact]
    public async Task Login_ShouldReturnCorrectRoles()
    {
        Context.Configurations.Add(new ConfigurationEntity()
        {
            Id = 1,
            Key = ConfigurationKey.AdminIdentifier.ToString(),
            Value = "123",
        });
        await Context.SaveChangesAsync();
        
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CorrectToken);

        var response = await Client.PostAsync("/api/user/login", new StringContent(
            JsonConvert.SerializeObject(new UserDto()),
            Encoding.UTF8,
            "application/json"
        ));

        Assert.Equal(200, (int)response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var loginResponse = JsonConvert.DeserializeObject<LoginResponse>(responseContent)!;
        
        Assert.NotNull(loginResponse);
        Assert.Equal(Role.User.ToString(), loginResponse.Roles.First());
    }
    
    [Fact]
    public async Task Login_ShouldReturnCorrectRoles_WhenEmptyAccessCode()
    {
        Context.Configurations.Add(new ConfigurationEntity()
        {
            Id = 1,
            Key = ConfigurationKey.AdminIdentifier.ToString(),
            Value = "123",
        });
        await Context.SaveChangesAsync();
        
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CorrectToken);

        var response = await Client.PostAsync("/api/user/login", new StringContent(
            JsonConvert.SerializeObject(new UserDto()
            {
                AccessCode = "",
                Id = -1,
            }),
            Encoding.UTF8,
            "application/json"
        ));

        Assert.Equal(200, (int)response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var loginResponse = JsonConvert.DeserializeObject<LoginResponse>(responseContent)!;
        
        Assert.NotNull(loginResponse);
        Assert.Equal(Role.User.ToString(), loginResponse.Roles.First());
    }
    
    [Fact]
    public async Task Login_ShouldAddCorrectDataToDatabase()
    {
        Context.Configurations.Add(new ConfigurationEntity()
        {
            Id = 1,
            Key = ConfigurationKey.AdminIdentifier.ToString(),
            Value = "123",
        });
        Context.Accesses.Add(new AccessEntity()
        {
            Code = "access-code",
            UserId = 1,
        });
        await Context.SaveChangesAsync();
        
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CorrectToken);

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/user/login");
        request.Content = new StringContent(
            JsonConvert.SerializeObject(new UserDto()
            {
                AccessCode = "access-code"
            }),
            Encoding.UTF8,
            "application/json"
        );

        request.Headers.Add("Accept-Language", "en-US");
        request.Headers.Add("User-Agent", "windows");
        request.Headers.Add("X-Forwarded-For", "192.158.1.38");

        await Client.SendAsync(request);
        
        var userEntity = await Context.Users.FindAsync(1);
        var ipLocationEntity = await Context.IpLocations.FindAsync(userEntity.IpLocationId);
        var accessEntity = await Context.Accesses.FindAsync(1);
        
        Assert.NotNull(userEntity);
        Assert.Equal(1, userEntity.Id);
        Assert.Equal("email", userEntity.Email);
        Assert.Equal(DateTime.UtcNow.Date, userEntity.LastLogin.Date);
        Assert.Equal("subject/id", userEntity.GoogleId);
        Assert.Equal(1, userEntity.IpLocationId);
        
        Assert.NotNull(ipLocationEntity);
        Assert.Equal("192.158.1.38", ipLocationEntity.Ip);
        Assert.Equal("US", ipLocationEntity.Country);
        Assert.Equal("California", ipLocationEntity.Region);
        Assert.Equal("Palo Alto", ipLocationEntity.City);
        Assert.Equal("94304", ipLocationEntity.Postal);
        Assert.Equal("America/Los_Angeles", ipLocationEntity.Timezone);
        Assert.Equal("37.4334,-122.1842", ipLocationEntity.Loc);

        Assert.NotNull(accessEntity);
        Assert.Equal(userEntity.Id, accessEntity.UserId);
    }
}
