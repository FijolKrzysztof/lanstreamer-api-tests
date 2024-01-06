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

public class AdminControllerTests : IntegrationTestsBase
{
    [Fact]
    public async Task UploadDesktopApp_ShouldNotAuthorize_WhenNoGoogleToken()
    {
        var fileBytes = Encoding.UTF8.GetBytes("This is a dummy file");
        var fileContent = new ByteArrayContent(fileBytes);

        using var formData = new MultipartFormDataContent();
        formData.Add(fileContent, "Data", "dummy.txt");

        var response = await Client.PostAsync("/api/admin/upload-desktop-app?operatingSystem=windows", formData);

        var responseContent = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(responseContent)!;

        Assert.Equal(401, (int)response.StatusCode);
        Assert.Equal("Missing google token", errorResponse.Message);
        Assert.Equal(401, errorResponse.StatusCode);
    }
    
    [Fact]
    public async Task UploadDesktopApp_ShouldNotAuthorize_WhenNoAdminRole()
    {
        Context.Configurations.Add(new ConfigurationEntity()
        {
            Id = 1,
            Key = ConfigurationKey.AdminIdentifier.ToString(),
            Value = "123"
        });
        await Context.SaveChangesAsync();
        
        var fileBytes = Encoding.UTF8.GetBytes("This is a dummy file");
        var fileContent = new ByteArrayContent(fileBytes);

        using var formData = new MultipartFormDataContent();
        formData.Add(fileContent, "Data", "dummy.txt");

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CorrectToken);

        var response = await Client.PostAsync("/api/admin/upload-desktop-app?operatingSystem=windows", formData);

        var responseContent = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(responseContent)!;

        Assert.Equal(401, (int)response.StatusCode);
        Assert.Equal("User doesn't have required role", errorResponse.Message);
        Assert.Equal(401, errorResponse.StatusCode);
    }

    [Fact]
    public async Task UploadDesktopApp_ShouldReturnError_WhenFileIsEmpty()
    {
        Context.Configurations.Add(new ConfigurationEntity()
        {
            Id = 1,
            Key = ConfigurationKey.AdminIdentifier.ToString(),
            Value = "subject/id"
        });
        await Context.SaveChangesAsync();

        var fileBytes = Encoding.UTF8.GetBytes("");
        var fileContent = new ByteArrayContent(fileBytes);
        
        using var formData = new MultipartFormDataContent();
        formData.Add(fileContent, "file", "dummy.txt");
        
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CorrectToken);

        var response = await Client.PostAsync("/api/admin/upload-desktop-app?operatingSystem=windows", formData);

        var responseContent = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(responseContent)!;

        Assert.Equal(400, (int)response.StatusCode);
        Assert.Equal("File is empty", errorResponse.Message);
        Assert.Equal(400, errorResponse.StatusCode);
    }
    
    [Fact]
    public async Task UploadDesktopApp_ShouldReturnError_WhenFileHasWrongFormat()
    {
        Context.Configurations.Add(new ConfigurationEntity()
        {
            Id = 1,
            Key = ConfigurationKey.AdminIdentifier.ToString(),
            Value = "subject/id"
        });
        await Context.SaveChangesAsync();

        var fileBytes = Encoding.UTF8.GetBytes("content");
        var fileContent = new ByteArrayContent(fileBytes);
        
        using var formData = new MultipartFormDataContent();
        formData.Add(fileContent, "file", "dummy.txt");
        
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CorrectToken);

        var response = await Client.PostAsync("/api/admin/upload-desktop-app?operatingSystem=windows", formData);

        var responseContent = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(responseContent)!;

        Assert.Equal(400, (int)response.StatusCode);
        Assert.Equal("Wrong file format. Accepting only ZIP files", errorResponse.Message);
        Assert.Equal(400, errorResponse.StatusCode);
    }

    [Fact]
    public async Task UploadDesktopApp_ShouldReturnError_WhenNoOperatingSystem()
    {
        Context.Configurations.Add(new ConfigurationEntity()
        {
            Id = 1,
            Key = ConfigurationKey.AdminIdentifier.ToString(),
            Value = "subject/id"
        });
        await Context.SaveChangesAsync();
        
        var fileBytes = Encoding.UTF8.GetBytes("content");
        var fileContent = new ByteArrayContent(fileBytes);
        
        using var formData = new MultipartFormDataContent();
        formData.Add(fileContent, "file", "dummy.txt");
        
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CorrectToken);

        var response = await Client.PostAsync("/api/admin/upload-desktop-app", formData);

        var responseContent = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(responseContent)!;
        
        Assert.Null(errorResponse.Message);
        Assert.Equal(400, (int)response.StatusCode);
    }
    
    [Fact]
    public async Task UploadDesktopApp_ShouldReturnCorrectStatusCode()
    {
        Context.Configurations.Add(new ConfigurationEntity()
        {
            Id = 1,
            Key = ConfigurationKey.AdminIdentifier.ToString(),
            Value = "subject/id"
        });
        Context.Configurations.Add(new ConfigurationEntity()
        {
            Id = 2,
            Key = ConfigurationKey.StoragePath.ToString(),
            Value = "temp"
        });
        Context.Configurations.Add(new ConfigurationEntity()
        {
            Id = 3,
            Key = ConfigurationKey.LanstreamerLinuxFilename.ToString(),
            Value = "lanstreamer-linux.zip"
        });
        Context.Configurations.Add(new ConfigurationEntity()
        {
            Id = 4,
            Key = ConfigurationKey.LanstreamerWindowsFilename.ToString(),
            Value = "lanstreamer-windows.zip"
        });
        await Context.SaveChangesAsync();

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

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CorrectToken);

        var response = await Client.PostAsync("/api/admin/upload-desktop-app?operatingSystem=windows", formData);

        Assert.Equal(200, (int)response.StatusCode);

        const string filePath = "temp/lanstreamer-windows.zip";
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }
    
    [Fact]
    public async Task UploadDesktopApp_ShouldSaveFile()
    {
        Context.Configurations.Add(new ConfigurationEntity()
        {
            Id = 1,
            Key = ConfigurationKey.AdminIdentifier.ToString(),
            Value = "subject/id"
        });
        Context.Configurations.Add(new ConfigurationEntity()
        {
            Id = 2,
            Key = ConfigurationKey.StoragePath.ToString(),
            Value = "temp"
        });
        Context.Configurations.Add(new ConfigurationEntity()
        {
            Id = 3,
            Key = ConfigurationKey.LanstreamerLinuxFilename.ToString(),
            Value = "lanstreamer-linux.zip"
        });
        await Context.SaveChangesAsync();

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

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CorrectToken);

        await Client.PostAsync("/api/admin/upload-desktop-app?operatingSystem=linux", formData);

        const string filePath = "temp/lanstreamer-linux.zip";
        var fileExists = File.Exists(filePath);

        Assert.True(fileExists);
        
        var fileHeader = new byte[4];
        using (var fileStream = File.OpenRead(filePath))
        {
            await fileStream.ReadAsync(fileHeader, 0, 4);
        }

        var isZipFileHeader = fileHeader[0] == 0x50 && fileHeader[1] == 0x4B;
        Assert.True(isZipFileHeader);
        
        File.Delete(filePath);
    }
}