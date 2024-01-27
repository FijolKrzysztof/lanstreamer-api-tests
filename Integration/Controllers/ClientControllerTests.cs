using System.Text;
using lanstreamer_api_tests.Integration.Abstract;
using lanstreamer_api.App.Data.Dto;
using lanstreamer_api.App.Data.Models.Enums;
using lanstreamer_api.Data.Configuration;
using lanstreamer_api.Entities;
using lanstreamer_api.Models;
using Newtonsoft.Json;

namespace lanstreamer_api_tests.Integration;

public class ClientControllerTests : IntegrationTestsBase
{
    [Fact]
    public async Task CreateClient_ShouldReturnCreatedObjResponse()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/client");
        request.Content = new StringContent(
            JsonConvert.SerializeObject(new ClientDto()),
            Encoding.UTF8,
            "application/json"
        );

        request.Headers.Add("Accept-Language", "en-US");
        request.Headers.Add("User-Agent", "windows");
        request.Headers.Add("X-Forwarded-For", "192.158.1.38");
        
        var response = await Client.SendAsync(request);
        
        Assert.Equal(201, (int)response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var createdObj = JsonConvert.DeserializeObject<CreatedObjResponse>(responseContent)!;
        
        Assert.Equal(1, createdObj.Id);
    }

    [Fact]
    public async Task CreateClient_ShouldAddCorrectDataToDatabase()
    {
        var clientDto = new ClientDto()
        {
            ReferrerWebsite = "website",
        };
        
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/client");
        request.Content = new StringContent(
            JsonConvert.SerializeObject(clientDto),
            Encoding.UTF8,
            "application/json"
        );

        request.Headers.Add("Accept-Language", "en-US");
        request.Headers.Add("User-Agent", "windows");
        request.Headers.Add("X-Forwarded-For", "192.158.1.38");
        
        var response = await Client.SendAsync(request);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var createdObj = JsonConvert.DeserializeObject<CreatedObjResponse>(responseContent)!;
        
        var clientEntity = await Context.Clients.FindAsync(createdObj.Id);
        var ipLocationEntity = await Context.IpLocations.FindAsync(clientEntity.IpLocationId);

        Assert.NotNull(clientEntity);
        Assert.Equal(1, clientEntity.Id);
        Assert.Equal("windows", clientEntity.OperatingSystem);
        Assert.Equal("en", clientEntity.Language);
        Assert.Equal("website", clientEntity.ReferrerWebsite);
        Assert.Equal(DateTime.Now.ToUniversalTime().Date, clientEntity.VisitTime.Date);
        Assert.Null(clientEntity.Downloads);
        Assert.Null(clientEntity.Feedbacks);
        Assert.Equal(1, clientEntity.IpLocationId);

        Assert.NotNull(ipLocationEntity);
        Assert.Equal("192.158.1.38", ipLocationEntity.Ip);
        Assert.Equal("US", ipLocationEntity.Country);
        Assert.Equal("California", ipLocationEntity.Region);
        Assert.Equal("Palo Alto", ipLocationEntity.City);
        Assert.Equal("94304", ipLocationEntity.Postal);
        Assert.Equal("America/Los_Angeles", ipLocationEntity.Timezone);
        Assert.Equal("37.4334,-122.1842", ipLocationEntity.Loc);
    }

    [Fact]
    public async Task AddFeedback_ShouldReturnCorrectStatusCode()
    {
        const string feedback = "feedback";

        Context.Clients.Add(new ClientEntity()
        {
            Id = 1,
            TimeOnSite = TimeSpan.Zero,
            OperatingSystem = "Windows",
            VisitTime = DateTime.Now.ToUniversalTime(),
            Language = "en",
        });
        await Context.SaveChangesAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/client/1/add-feedback");
        request.Content = new StringContent(
            JsonConvert.SerializeObject(feedback),
            Encoding.UTF8,
            "application/json"
        );

        var response = await Client.SendAsync(request);
        
        Assert.Equal(200, (int)response.StatusCode);
    }

    [Fact]
    public async Task AddFeedback_ShouldAddCorrectDataToDatabase()
    {
        const string feedback = "feedback";

        Context.Clients.Add(new ClientEntity()
        {
            Id = 1,
            TimeOnSite = TimeSpan.Zero,
            OperatingSystem = "Windows",
            VisitTime = DateTime.UtcNow,
            Language = "en",
        });
        await Context.SaveChangesAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/client/1/add-feedback");
        request.Content = new StringContent(
            JsonConvert.SerializeObject(feedback),
            Encoding.UTF8,
            "application/json"
        );

        await Client.SendAsync(request);
        
        var clientEntity = Context.Clients.First();
        var feedbackEntities = Context.Feedbacks.Where(f => f.ClientId == clientEntity.Id).ToList();

        Assert.NotNull(clientEntity);
        Assert.Single(feedbackEntities);

        Assert.Equal(1, feedbackEntities[0].Id);
        Assert.Equal("feedback", feedbackEntities[0].Message);
    }

    [Fact]
    public async Task UpdateSessionDuration_ShouldReturnCorrectStatusCode()
    {
        Context.Clients.Add(new ClientEntity()
        {
            Id = 1,
            TimeOnSite = TimeSpan.Zero,
            OperatingSystem = "Windows",
            VisitTime = DateTime.UtcNow,
            Language = "en",
        });
        await Context.SaveChangesAsync();
        
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/client/1/update-session-duration");
        request.Content = new StringContent(
            "",
            Encoding.UTF8,
            "application/json"
        );

        var response = await Client.SendAsync(request);
        
        Assert.Equal(200, (int)response.StatusCode);
    }

    [Fact]
    public async Task UpdateSessionDuration_ShouldUpdateCorrectDataInDatabase()
    {
        Context.Clients.Add(new ClientEntity()
        {
            Id = 1,
            TimeOnSite = TimeSpan.Zero,
            OperatingSystem = "Windows",
            VisitTime = DateTime.UtcNow,
            Language = "en",
        });
        await Context.SaveChangesAsync();
        
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/client/1/update-session-duration");
        request.Content = new StringContent(
            "",
            Encoding.UTF8,
            "application/json"
        );

        await Client.SendAsync(request);
        
        var resultDbObj = await Context.Clients.FindAsync(1);

        Assert.NotNull(resultDbObj);
        Assert.Equal(TimeSpan.Zero.TotalMinutes, (int)resultDbObj.TimeOnSite.TotalMinutes);
    }

    [Fact]
    public async Task DownloadApp_ShouldReturnFile()
    {
        Context.Clients.Add(new ClientEntity()
        {
            Id = 1,
            TimeOnSite = TimeSpan.Zero,
            OperatingSystem = "Windows",
            VisitTime = DateTime.UtcNow,
            Language = "en",
        });
        Context.Configurations.Add(new ConfigurationEntity()
        {
            Id = 1,
            Key = ConfigurationKey.StoragePath.ToString(),
            Value = "temp"
        });
        Context.Configurations.Add(new ConfigurationEntity()
        {
            Id = 2,
            Key = ConfigurationKey.LanstreamerLinuxFilename.ToString(),
            Value = "lanstreamer-linux.zip"
        });
        Context.Configurations.Add(new ConfigurationEntity()
        {
            Id = 3,
            Key = ConfigurationKey.LanstreamerWindowsFilename.ToString(),
            Value = "lanstreamer-windows.zip"
        });
        await Context.SaveChangesAsync();
        
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/client/1/download-app/windows");
        request.Content = new StringContent(
            "",
            Encoding.UTF8,
            "application/octet-stream"
        );

        const string tempFilePath = "temp/lanstreamer-windows.zip";
        var content = Encoding.UTF8.GetBytes("This is a temporary file content.");

        string directoryPath = Path.GetDirectoryName(tempFilePath);
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
        
        await File.WriteAllBytesAsync(tempFilePath, content);
        
        var response = await Client.SendAsync(request);
        
        Assert.Equal(200, (int)response.StatusCode);
        
        var result = await response.Content.ReadAsByteArrayAsync();
        
        Assert.Equal(content, result);
        
        if (response.Content.Headers.TryGetValues("Content-Disposition", out IEnumerable<string>? contentDisposition))
        {
            var header = contentDisposition.FirstOrDefault();
            if (header != null)
            {
                Assert.Contains("filename=lanstreamer.zip", header);
            }
        }

        if (Directory.Exists(directoryPath))
        {
            Directory.Delete(directoryPath, true);
        }
    }

    [Fact]
    public async Task DownloadApp_ShouldUpdateCorrectDataInDatabase()
    {
        Context.Clients.Add(new ClientEntity()
        {
            Id = 1,
            TimeOnSite = TimeSpan.Zero,
            OperatingSystem = "Windows",
            VisitTime = DateTime.UtcNow,
            Language = "en",
        });
        Context.Configurations.Add(new ConfigurationEntity()
        {
            Id = 1,
            Key = ConfigurationKey.StoragePath.ToString(),
            Value = "temp"
        });
        Context.Configurations.Add(new ConfigurationEntity()
        {
            Id = 2,
            Key = ConfigurationKey.LanstreamerWindowsFilename.ToString(),
            Value = "lanstreamer-windows.zip"
        });
        await Context.SaveChangesAsync();
        
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/client/1/download-app/windows");
        request.Content = new StringContent(
            "",
            Encoding.UTF8,
            "application/json"
        );
        
        const int clientId = 1;
        const string tempFilePath = "temp/lanstreamer-windows.zip";
        var content = Encoding.UTF8.GetBytes("This is a temporary file content.");

        string directoryPath = Path.GetDirectoryName(tempFilePath);
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
        
        await File.WriteAllBytesAsync(tempFilePath, content);

        await Client.SendAsync(request);
        
        var resultDbObj = await Context.Clients.FindAsync(clientId);
        await Context.Entry(resultDbObj).ReloadAsync(); 

        Assert.NotNull(resultDbObj);
        Assert.Equal(1, resultDbObj.Downloads);
        
        if (Directory.Exists(directoryPath))
        {
            Directory.Delete(directoryPath, true);
        }
    }

    [Fact]
    public async Task CheckAppPresence_ShouldReturnNotFound()
    {
        var request = new HttpRequestMessage(HttpMethod.Head, $"/api/client/1/download-app/windows");
        request.Content = new StringContent(
            "",
            Encoding.UTF8,
            "application/json"
        );
        
        var response = await Client.SendAsync(request);
        
        Assert.Equal(404, (int)response.StatusCode);
    }
    
    [Fact]
    public async Task CheckAppPresence_ShouldReturnOk()
    {
        Context.Clients.Add(new ClientEntity()
        {
            Id = 1,
            TimeOnSite = TimeSpan.Zero,
            OperatingSystem = "Windows",
            VisitTime = DateTime.UtcNow,
            Language = "en",
        });
        Context.Configurations.Add(new ConfigurationEntity()
        {
            Id = 1,
            Key = ConfigurationKey.StoragePath.ToString(),
            Value = "temp"
        });
        Context.Configurations.Add(new ConfigurationEntity()
        {
            Id = 2,
            Key = ConfigurationKey.LanstreamerWindowsFilename.ToString(),
            Value = "lanstreamer-windows.zip"
        });
        await Context.SaveChangesAsync();
        
        var request = new HttpRequestMessage(HttpMethod.Head, $"/api/client/1/download-app/windows");
        request.Content = new StringContent(
            "",
            Encoding.UTF8,
            "application/json"
        );
        
        const string tempFilePath = "temp/lanstreamer-windows.zip";
        var content = Encoding.UTF8.GetBytes("This is a temporary file content.");

        var directoryPath = Path.GetDirectoryName(tempFilePath);
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath!);
        }
        
        await File.WriteAllBytesAsync(tempFilePath, content);
        
        var response = await Client.SendAsync(request);
        
        Assert.Equal(200, (int)response.StatusCode);
        
        if (Directory.Exists(directoryPath))
        {
            Directory.Delete(directoryPath, true);
        }
    }
    
    [Fact]
    public async Task CheckAppPresence_ShouldNotUpdateDownloads()
    {
        Context.Clients.Add(new ClientEntity()
        {
            Id = 1,
            TimeOnSite = TimeSpan.Zero,
            OperatingSystem = "Windows",
            VisitTime = DateTime.UtcNow,
            Language = "en",
        });
        Context.Configurations.Add(new ConfigurationEntity()
        {
            Id = 1,
            Key = ConfigurationKey.StoragePath.ToString(),
            Value = "temp"
        });
        Context.Configurations.Add(new ConfigurationEntity()
        {
            Id = 2,
            Key = ConfigurationKey.LanstreamerWindowsFilename.ToString(),
            Value = "lanstreamer-windows.zip"
        });
        await Context.SaveChangesAsync();
        
        var request = new HttpRequestMessage(HttpMethod.Head, $"/api/client/1/download-app/windows");
        request.Content = new StringContent(
            "",
            Encoding.UTF8,
            "application/json"
        );
        
        const string tempFilePath = "temp/lanstreamer-windows.zip";
        var content = Encoding.UTF8.GetBytes("This is a temporary file content.");

        var directoryPath = Path.GetDirectoryName(tempFilePath);
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath!);
        }
        
        await File.WriteAllBytesAsync(tempFilePath, content);
        
        await Client.SendAsync(request);
        
        var resultDbObj = await Context.Clients.FindAsync(1);

        Assert.NotNull(resultDbObj);
        Assert.Null(resultDbObj.Downloads);
        
        if (Directory.Exists(directoryPath))
        {
            Directory.Delete(directoryPath, true);
        }
    }
}