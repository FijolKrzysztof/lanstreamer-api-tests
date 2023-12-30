using System.Net.Http.Headers;
using System.Text;
using lanstreamer_api_tests.Integration.Abstract;
using lanstreamer_api.App.Data.Dto.Responses;
using lanstreamer_api.App.Data.Models.Enums;
using lanstreamer_api.Data.Configuration;
using lanstreamer_api.Models;
using Newtonsoft.Json;

namespace lanstreamer_api_tests.Integration;

public class DesktopAppControllerTests : ControllerTestsBase
{
    [Fact]
    public async Task ShouldThrowAnError_WhenWrongAppVersion()
    {
        _context.Configurations.Add(new ConfigurationEntity()
        {
            Id = 1,
            Key = ConfigurationKey.DesktopAppVersion.ToString(),
            Value = "2.0"
        });
        await _context.SaveChangesAsync();
        
        // TODO: dodać części uwspólnione

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/desktop-app/access?accessCode=123&version=1.0");
        request.Content = new StringContent(
            "",
            Encoding.UTF8,
            "text/event-stream"
        );

        var response = await _client.SendAsync(request);

        Assert.Equal(401, (int)response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(responseContent)!;

        Assert.Equal("Version is not supported", errorResponse.Message);
        Assert.Equal(401, errorResponse.StatusCode);
    }

    [Fact]
    public async Task ShouldPassTimeout()
    {
        _context.Configurations.Add(new ConfigurationEntity()
        {
            Id = 1,
            Key = ConfigurationKey.DesktopAppVersion.ToString(),
            Value = "1.0"
        });
        _context.Configurations.Add(new ConfigurationEntity()
        {
            Id = 2,
            Key = ConfigurationKey.LoginTimeoutSeconds.ToString(),
            Value = "1"
        });
        await _context.SaveChangesAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/desktop-app/access?accessCode=123&version=1.0");
        request.Content = new StringContent(
            "",
            Encoding.UTF8,
            "text/event-stream"
        );

        var response = await _client.SendAsync(request);
        
        Assert.Equal(408, (int)response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(responseContent)!;
        
        Assert.Equal(408, errorResponse.StatusCode);
        Assert.Equal("Timeout waiting for user login", errorResponse.Message);
    }

    [Fact]
    public async Task ShouldReturnTrue()
    {
        _context.Configurations.Add(new ConfigurationEntity()
        {
            Id = 1,
            Key = ConfigurationKey.DesktopAppVersion.ToString(),
            Value = "1.0"
        });
        _context.Configurations.Add(new ConfigurationEntity()
        {
            Id = 2,
            Key = ConfigurationKey.LoginTimeoutSeconds.ToString(),
            Value = "60"
        });
        _context.Configurations.Add(new ConfigurationEntity()
        {
            Id = 3,
            Key = ConfigurationKey.OfflineLogins.ToString(),
            Value = "3"
        });
        await _context.SaveChangesAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/desktop-app/access?accessCode=123&version=1.0");
        request.Content = new StringContent(
            "",
            Encoding.UTF8,
            "text/event-stream"
        );

        var task = Task.Run(async () =>
        {
            var response = await _client.SendAsync(request);
            
            await using var stream = await response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(stream);

            var cancellationTokenSource =
                new CancellationTokenSource(TimeSpan.FromSeconds(10));

            try
            {
                while (!cancellationTokenSource.Token.IsCancellationRequested)
                {
                    var line = await reader.ReadLineAsync();
                    
                    Assert.Equal(3, int.Parse(line));
                    return;
                }
            }
            catch (OperationCanceledException)
            {
                Assert.Fail();
            }
            finally
            {
                cancellationTokenSource.Dispose();
            }
        });

        const string accessToken = "correct-token";

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        await _client.PostAsync("/api/user/login", new StringContent(
            JsonConvert.SerializeObject(new UserDto()
            {
                AccessCode = "123"
            }),
            Encoding.UTF8,
            "application/json"
        ));

        await task;
    }
}
