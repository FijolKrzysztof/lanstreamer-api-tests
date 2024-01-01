using System.Net.Http.Headers;
using System.Text;
using lanstreamer_api_tests.Integration.Abstract;
using lanstreamer_api.App.Data.Dto.Responses;
using lanstreamer_api.App.Data.Models.Enums;
using lanstreamer_api.Models;
using lanstreamer_api.Models.Responses;
using Newtonsoft.Json;

namespace lanstreamer_api_tests.Integration;

public class UserControllerTests : IntegrationTestsBase
{
    [Fact]
    public async Task Login_ShouldNotAuthorize_WhenNoGoogleId()
    {
        var response = await _client.PostAsync("/api/user/login", new StringContent(
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

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _client.PostAsync("/api/user/login", new StringContent(
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
        const string accessToken = "correct-token";
        
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _client.PostAsync("/api/user/login", new StringContent(
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
    public async Task Login_ShouldAddCorrectDataToDatabase()
    {
        const string accessToken = "correct-token";
        
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

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

        await _client.SendAsync(request);
        
        var userEntity = await _context.Users.FindAsync(1);
        var ipLocationEntity = await _context.IpLocations.FindAsync(userEntity.IpLocationId);
        
        Assert.NotNull(userEntity);
        Assert.Equal(1, userEntity.Id);
        Assert.Equal("email", userEntity.Email);
        Assert.Equal(DateTime.UtcNow.Date, userEntity.LastLogin.Date);
        Assert.Equal("subject/id", userEntity.GoogleId);
        Assert.Equal("access-code", userEntity.AccessCode);
        Assert.Equal(1, userEntity.IpLocationId);
        
        Assert.NotNull(ipLocationEntity);
        Assert.Equal("192.158.1.38", ipLocationEntity.Ip);
        Assert.Equal("US", ipLocationEntity.Country);
        Assert.Equal("California", ipLocationEntity.Region);
        Assert.Equal("Palo Alto", ipLocationEntity.City);
        Assert.Equal("94304", ipLocationEntity.Postal);
        Assert.Equal("America/Los_Angeles", ipLocationEntity.Timezone);
        Assert.Equal("37.4334,-122.1842", ipLocationEntity.Loc);
    }
}
