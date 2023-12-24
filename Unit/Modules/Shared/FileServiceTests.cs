using lanstreamer_api.services.FileService;

namespace lanstreamer_api_tests.Unit.Modules.Shared;

public class FileServiceTests
{
    private readonly FileService _fileService;
    
    public FileServiceTests()
    {
        _fileService = new FileService();
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
}