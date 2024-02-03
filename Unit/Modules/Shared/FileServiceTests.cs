using lanstreamer_api.App.Data.Models.Enums;
using lanstreamer_api.Data.Configuration;
using lanstreamer_api.services.FileService;
using Moq;
using OperatingSystem = lanstreamer_api.App.Data.Models.Enums.OperatingSystem;

namespace lanstreamer_api_tests.Unit.Modules.Shared;

public class FileServiceTests
{
    private readonly FileService _fileService;
    
    public FileServiceTests()
    {
        var configurationRepository = new Mock<IConfigurationRepository>();
        
        _fileService = new FileService(configurationRepository.Object);
    }
    
    [Fact]
    public void Exists_ShouldReturnTrue_WhenFileExists()
    {
        var path = Path.GetTempFileName();
        
        Assert.True(_fileService.Exists(path));
    }
    
    [Fact]
    public void Exists_ShouldReturnFalse_WhenFileDoesNotExist()
    {
        var path = Path.GetTempFileName();
        File.Delete(path);
        
        Assert.False(_fileService.Exists(path));
    }
    
    [Fact]
    public void ReadFileStream_ShouldReturnFileStream()
    {
        var path = Path.GetTempFileName();
        var fileStream = _fileService.ReadFileStream(path);
        
        Assert.IsType<FileStream>(fileStream);
    }
    
    [Fact]
    public void ReadFileStream_ShouldReturnFileStreamWithCorrectPath()
    {
        var path = Path.GetTempFileName();
        var fileStream = _fileService.ReadFileStream(path);
        
        Assert.Equal(path, fileStream.Name);
    }
    
    [Fact]
    public async Task GetDesktopAppPath_ShouldReturnCorrectPath_WhenWindows()
    {
        var configurationRepository = new Mock<IConfigurationRepository>();
        configurationRepository.Setup(x => x.GetByKey(ConfigurationKey.StoragePath))
            .ReturnsAsync("/test/path");
        configurationRepository.Setup(x => x.GetByKey(ConfigurationKey.LanstreamerWindowsFilename))
            .ReturnsAsync("test.zip");
        
        var fileService = new FileService(configurationRepository.Object);
        
        var path = await fileService.GetDesktopAppPath(OperatingSystem.Windows);
        
        Assert.Equal("/test/path/test.zip", path);
    }
    
    [Fact]
    public async Task GetDesktopAppPath_ShouldReturnCorrectPath_WhenLinux()
    {
        var configurationRepository = new Mock<IConfigurationRepository>();
        configurationRepository.Setup(x => x.GetByKey(ConfigurationKey.StoragePath))
            .ReturnsAsync("/test/path");
        configurationRepository.Setup(x => x.GetByKey(ConfigurationKey.LanstreamerLinuxFilename))
            .ReturnsAsync("test.zip");
        
        var fileService = new FileService(configurationRepository.Object);
        
        var path = await fileService.GetDesktopAppPath(OperatingSystem.Linux);
        
        Assert.Equal("/test/path/test.zip", path);
    }
}