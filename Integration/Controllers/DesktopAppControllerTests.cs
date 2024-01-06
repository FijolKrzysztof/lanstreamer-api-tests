using System.Net.Http.Headers;
using System.Text;
using lanstreamer_api_tests.Integration.Abstract;
using lanstreamer_api.App.Data.Dto.Responses;
using lanstreamer_api.App.Data.Models.Enums;
using lanstreamer_api.Data.Configuration;
using lanstreamer_api.Models;
using Newtonsoft.Json;

namespace lanstreamer_api_tests.Integration.Controllers;

public class DesktopAppControllerTests : IntegrationTestsBase
{
    [Fact]
    public async Task Access_ShouldThrowAnError_WhenWrongAppVersion()
    {
        Context.Configurations.Add(new ConfigurationEntity()
        {
            Id = 1,
            Key = ConfigurationKey.DesktopAppVersion.ToString(),
            Value = "2.0"
        });
        await Context.SaveChangesAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/desktop-app/access?accessCode=123&version=1.0");
        request.Content = new StringContent(
            "",
            Encoding.UTF8,
            "text/event-stream"
        );

        var response = await Client.SendAsync(request);

        Assert.Equal(401, (int)response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(responseContent)!;

        Assert.Equal("Version is not supported", errorResponse.Message);
        Assert.Equal(401, errorResponse.StatusCode);
    }

    [Fact]
    public async Task Access_ShouldPassTimeout()
    {
        Context.Configurations.Add(new ConfigurationEntity()
        {
            Id = 1,
            Key = ConfigurationKey.DesktopAppVersion.ToString(),
            Value = "1.0"
        });
        Context.Configurations.Add(new ConfigurationEntity()
        {
            Id = 2,
            Key = ConfigurationKey.LoginTimeoutSeconds.ToString(),
            Value = "1"
        });
        await Context.SaveChangesAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/desktop-app/access?accessCode=123&version=1.0");
        request.Content = new StringContent(
            "",
            Encoding.UTF8,
            "text/event-stream"
        );

        var response = await Client.SendAsync(request);

        Assert.Equal(408, (int)response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(responseContent)!;

        Assert.Equal(408, errorResponse.StatusCode);
        Assert.Equal("Timeout waiting for user login", errorResponse.Message);
    }

    [Fact]
    public async Task Access_ShouldReturnNumber()
    {
        Context.Configurations.Add(new ConfigurationEntity()
        {
            Id = 1,
            Key = ConfigurationKey.DesktopAppVersion.ToString(),
            Value = "1.0"
        });
        Context.Configurations.Add(new ConfigurationEntity()
        {
            Id = 2,
            Key = ConfigurationKey.LoginTimeoutSeconds.ToString(),
            Value = "3"
        });
        Context.Configurations.Add(new ConfigurationEntity()
        {
            Id = 3,
            Key = ConfigurationKey.OfflineLogins.ToString(),
            Value = "3"
        });
        Context.Configurations.Add(new ConfigurationEntity()
        {
            Id = 4,
            Key = ConfigurationKey.AdminIdentifier.ToString(),
            Value = "123"
        });
        await Context.SaveChangesAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/desktop-app/access?accessCode=123&version=1.0");
        request.Content = new StringContent(
            "",
            Encoding.UTF8,
            "text/event-stream"
        );

        var task = Task.Run(async () =>
        {
            var response = await Client.SendAsync(request);

            await using var stream = await response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(stream);

            var cancellationTokenSource =
                new CancellationTokenSource(TimeSpan.FromSeconds(10));

            try
            {
                var line = await reader.ReadLineAsync(cancellationTokenSource.Token);

                Assert.NotNull(line);
                Assert.Equal(3, int.Parse(line));
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

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CorrectToken);

        await Client.PostAsync("/api/user/login", new StringContent(
            JsonConvert.SerializeObject(new UserDto()
            {
                AccessCode = "123"
            }),
            Encoding.UTF8,
            "application/json"
        ));

        await task;
    }

    [Fact]
    public async Task Access_ShouldUpdateAppVersion()
    {
        Context.Configurations.Add(new ConfigurationEntity()
        {
            Id = 1,
            Key = ConfigurationKey.DesktopAppVersion.ToString(),
            Value = "1.0"
        });
        Context.Configurations.Add(new ConfigurationEntity()
        {
            Id = 2,
            Key = ConfigurationKey.LoginTimeoutSeconds.ToString(),
            Value = "3"
        });
        Context.Configurations.Add(new ConfigurationEntity()
        {
            Id = 3,
            Key = ConfigurationKey.OfflineLogins.ToString(),
            Value = "3"
        });
        Context.Configurations.Add(new ConfigurationEntity()
        {
            Id = 4,
            Key = ConfigurationKey.AdminIdentifier.ToString(),
            Value = "123"
        });
        await Context.SaveChangesAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/desktop-app/access?accessCode=123&version=1.0");
        request.Content = new StringContent(
            "",
            Encoding.UTF8,
            "text/event-stream"
        );

        var task = Task.Run(async () =>
        {
            await Client.SendAsync(request);

            var user = await Context.Users.FindAsync(1);

            Assert.NotNull(user);
            Assert.Equal(1, user.AppVersion);
        });

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CorrectToken);

        await Client.PostAsync("/api/user/login", new StringContent(
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