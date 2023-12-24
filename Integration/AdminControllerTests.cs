using System.IO.Compression;
using System.Net.Http.Headers;
using System.Text;
using lanstreamer_api_tests.Integration.Abstract;
using lanstreamer_api.App.Data.Dto.Responses;
using lanstreamer_api.App.Data.Models.Enums;
using lanstreamer_api.Data.Configuration;
using lanstreamer_api.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using OperatingSystem = lanstreamer_api.App.Data.Models.Enums.OperatingSystem;

namespace lanstreamer_api_tests.Integration;

public class AdminControllerTests : ControllerTestsBase
{
    [Fact]
    public async Task UploadDesktopApp_ShouldNotAuthorize_WhenNoGoogleToken()
    {
        var fileBytes = Encoding.UTF8.GetBytes("This is a dummy file");
        var fileContent = new ByteArrayContent(fileBytes);

        using var formData = new MultipartFormDataContent();
        formData.Add(fileContent, "Data", "dummy.txt");

        var response = await _client.PostAsync("/api/admin/upload-desktop-app?operatingSystem=windows", formData);

        var responseContent = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(responseContent)!;

        Assert.Equal(401, (int)response.StatusCode);
        Assert.Equal("Missing google token", errorResponse.Message);
        Assert.Equal(401, errorResponse.StatusCode);
    }

    [Fact]
    public async Task UploadDesktopApp_ShouldNotAuthorize_WhenNoAdminRole()
    {
        var fileBytes = Encoding.UTF8.GetBytes("This is a dummy file");
        var fileContent = new ByteArrayContent(fileBytes);

        using var formData = new MultipartFormDataContent();
        formData.Add(fileContent, "Data", "dummy.txt");

        const string accessToken = "correct-token";

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _client.PostAsync("/api/admin/upload-desktop-app?operatingSystem=windows", formData);

        var responseContent = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(responseContent)!;

        Assert.Equal(401, (int)response.StatusCode);
        Assert.Equal("User doesn't have required role", errorResponse.Message);
        Assert.Equal(401, errorResponse.StatusCode);
    }

    [Fact]
    public async Task UploadDesktopApp_ShouldReturnError_WhenFileIsEmpty()
    {
        _context.Configurations.Add(new ConfigurationEntity()
        {
            Id = 1,
            Key = ConfigurationKey.AdminIdentifier,
            Value = "subject/id"
        });
        await _context.SaveChangesAsync();

        var fileBytes = Encoding.UTF8.GetBytes("");
        var fileContent = new ByteArrayContent(fileBytes);
        
        using var formData = new MultipartFormDataContent();
        formData.Add(fileContent, "file", "dummy.txt");
        
        const string accessToken = "correct-token";

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _client.PostAsync("/api/admin/upload-desktop-app?operatingSystem=windows", formData);

        var responseContent = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(responseContent)!;

        Assert.Equal(400, (int)response.StatusCode);
        Assert.Equal("File is empty", errorResponse.Message);
        Assert.Equal(400, errorResponse.StatusCode);
    }
    
    [Fact]
    public async Task UploadDesktopApp_ShouldReturnError_WhenFileHasWrongFormat()
    {
        _context.Configurations.Add(new ConfigurationEntity()
        {
            Id = 1,
            Key = ConfigurationKey.AdminIdentifier,
            Value = "subject/id"
        });
        await _context.SaveChangesAsync();

        var fileBytes = Encoding.UTF8.GetBytes("content");
        var fileContent = new ByteArrayContent(fileBytes);
        
        using var formData = new MultipartFormDataContent();
        formData.Add(fileContent, "file", "dummy.txt");
        
        const string accessToken = "correct-token";

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _client.PostAsync("/api/admin/upload-desktop-app?operatingSystem=windows", formData);

        var responseContent = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(responseContent)!;

        Assert.Equal(400, (int)response.StatusCode);
        Assert.Equal("Wrong file format. Accepting only ZIP files", errorResponse.Message);
        Assert.Equal(400, errorResponse.StatusCode);
    }

    [Fact]
    public async Task UploadDesktopApp_ShouldReturnError_WhenNoOperatingSystem()
    {
        _context.Configurations.Add(new ConfigurationEntity()
        {
            Id = 1,
            Key = ConfigurationKey.AdminIdentifier,
            Value = "subject/id"
        });
        await _context.SaveChangesAsync();
        
        var fileBytes = Encoding.UTF8.GetBytes("content");
        var fileContent = new ByteArrayContent(fileBytes);
        
        using var formData = new MultipartFormDataContent();
        formData.Add(fileContent, "file", "dummy.txt");
        
        const string accessToken = "correct-token";

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _client.PostAsync("/api/admin/upload-desktop-app", formData);

        var responseContent = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(responseContent)!;
        
        Assert.Null(errorResponse.Message);
        Assert.Equal(400, (int)response.StatusCode);
    }
    
    [Fact]
    public async Task UploadDesktopApp_ShouldReturnCorrectStatusCode()
    {
        _context.Configurations.Add(new ConfigurationEntity()
        {
            Id = 1,
            Key = ConfigurationKey.AdminIdentifier,
            Value = "subject/id"
        });
        await _context.SaveChangesAsync();

        var filesToZip = new Dictionary<string, byte[]>
        {
            { "file1.txt", Encoding.UTF8.GetBytes("content 1") },
            { "file2.txt", Encoding.UTF8.GetBytes("content 2") }
        };

        byte[] zipFileBytes;
        using (var memoryStream = new MemoryStream())
        {
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                foreach (var file in filesToZip)
                {
                    var entry = archive.CreateEntry(file.Key, CompressionLevel.Fastest);
                    using (var entryStream = entry.Open())
                    {
                        await entryStream.WriteAsync(file.Value, 0, file.Value.Length);
                    }
                }
            }

            zipFileBytes = memoryStream.ToArray();
        }
        
        var zipContent = new ByteArrayContent(zipFileBytes);

        using var formData = new MultipartFormDataContent();
        formData.Add(zipContent, "file", "server_file.zip");

        const string accessToken = "correct-token";
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _client.PostAsync("/api/admin/upload-desktop-app?operatingSystem=windows", formData);

        Assert.Equal(200, (int)response.StatusCode);
        
        var filePath = ApplicationBuildPath.GetPath(OperatingSystem.Linux);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }
    
    [Fact]
    public async Task UploadDesktopApp_ShouldSaveFile()
    {
        _context.Configurations.Add(new ConfigurationEntity()
        {
            Id = 1,
            Key = ConfigurationKey.AdminIdentifier,
            Value = "subject/id"
        });
        await _context.SaveChangesAsync();

        var filesToZip = new Dictionary<string, byte[]>
        {
            { "file1.txt", Encoding.UTF8.GetBytes("content 1") },
            { "file2.txt", Encoding.UTF8.GetBytes("content 2") }
        };

        byte[] zipFileBytes;
        using (var memoryStream = new MemoryStream())
        {
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                foreach (var file in filesToZip)
                {
                    var entry = archive.CreateEntry(file.Key, CompressionLevel.Fastest);
                    using (var entryStream = entry.Open())
                    {
                        await entryStream.WriteAsync(file.Value, 0, file.Value.Length);
                    }
                }
            }

            zipFileBytes = memoryStream.ToArray();
        }
        
        var zipContent = new ByteArrayContent(zipFileBytes);

        using var formData = new MultipartFormDataContent();
        formData.Add(zipContent, "file", "server_file.zip");

        const string accessToken = "correct-token";
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        await _client.PostAsync("/api/admin/upload-desktop-app?operatingSystem=windows", formData);

        var filePath = ApplicationBuildPath.GetPath(OperatingSystem.Linux);
        var fileExists = File.Exists(filePath);

        Assert.True(fileExists); // TODO: coś jest nie tak bo tego pliku nie ma z tego co sprawdzałem
        
        if (fileExists)
        {
            File.Delete(filePath);
        }
    }
}