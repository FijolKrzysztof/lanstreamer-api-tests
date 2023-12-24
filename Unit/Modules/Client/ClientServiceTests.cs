using lanstreamer_api.App.Client;
using lanstreamer_api.App.Exceptions;
using lanstreamer_api.Data.Modules.Client;
using lanstreamer_api.Entities;
using lanstreamer_api.Models;
using lanstreamer_api.services;
using lanstreamer_api.services.FileService;
using Microsoft.AspNetCore.Http;
using Moq;
using OperatingSystem = lanstreamer_api.App.Data.Models.Enums.OperatingSystem;

namespace lanstreamer_api_tests.Unit.Modules.Client;

public class ClientServiceTests
{
    private readonly ClientService _clientService;
    private readonly Mock<IClientRepository> _clientRepository;
    private readonly Mock<IFileService> _fileService;
    private readonly Mock<IFeedbackRepository> _feedbackRepository;

    public ClientServiceTests()
    {
        var clientConverter = new ClientConverter();
        
        _fileService = new Mock<IFileService>();
        _clientRepository = new Mock<IClientRepository>();
        _feedbackRepository = new Mock<IFeedbackRepository>();
        var httpRequestInfoService = new Mock<IHttpRequestInfoService>();

        _clientService = new ClientService(_feedbackRepository.Object, clientConverter, _clientRepository.Object, httpRequestInfoService.Object, _fileService.Object);
    }

    [Fact]
    public async Task CreateClient_ShouldReturnCorrectClientId()
    {
        var clientDto = new ClientDto() { Id = 5 };

        _clientRepository.Setup(repo => repo.Create(It.IsAny<ClientEntity>())).ReturnsAsync(new ClientEntity()
        {
            Id = 1,
        });

        var response = await _clientService.CreateClient(clientDto, new Mock<HttpContext>().Object);

        Assert.Equal(1, response.Id);
    }

    [Fact]
    public async Task CreateClient_ShouldPassReferrerWebsiteToRepository()
    {
        var clientDto = new ClientDto() { ReferrerWebsite = "referrerWebsite" };

        _clientRepository.Setup(repo => repo.Create(It.IsAny<ClientEntity>()))
            .ReturnsAsync(new ClientEntity() { Id = 1 })
            .Callback<ClientEntity>(clientEntity =>
            {
                Assert.Equal(clientDto.ReferrerWebsite, clientEntity.ReferrerWebsite);
            });
        
        await _clientService.CreateClient(clientDto, new Mock<HttpContext>().Object);
    }
    
    [Fact]
    public async Task CreateClient_ShouldPassVisitTimeToRepository()
    {
        var clientDto = new ClientDto();

        _clientRepository.Setup(repo => repo.Create(It.IsAny<ClientEntity>()))
            .ReturnsAsync(new ClientEntity() { Id = 1 })
            .Callback<ClientEntity>(clientEntity =>
            {
                Assert.Equal(DateTime.Now.ToUniversalTime().Date, clientEntity.VisitTime.Date);
            });
        
        await _clientService.CreateClient(clientDto, new Mock<HttpContext>().Object);
    }
    
    [Fact]
    public async Task CreateClient_ShouldPassTimeOnSiteToRepository()
    {
        var clientDto = new ClientDto();

        _clientRepository.Setup(repo => repo.Create(It.IsAny<ClientEntity>()))
            .ReturnsAsync(new ClientEntity() { Id = 1 })
            .Callback<ClientEntity>(clientEntity =>
            {
                Assert.Equal(TimeSpan.Zero, clientEntity.TimeOnSite);
            });
        
        await _clientService.CreateClient(clientDto, new Mock<HttpContext>().Object);
    }

    [Fact]
    public async Task AddFeedbacks_ShouldThrowAnErrorWhenIdIsNotCorrect()
    {
        _clientRepository.Setup(repo => repo.GetById(It.IsAny<int>()))
            .ReturnsAsync((ClientEntity) null);
        
        await Assert.ThrowsAsync<AppException>(() => _clientService.AddFeedbacks(2, new List<string>()));
    }
    
    [Fact]
    public async Task AddFeedbacks_ShouldAddFeedbacks()
    {
        var clientId = 1;
        var feedbacks = new List<string>() { "feedback1", "feedback2" };

        _clientRepository.Setup(repo => repo.GetById(It.IsAny<int>()))
            .ReturnsAsync(new ClientEntity() { Id = 1 });

        _feedbackRepository.Setup(repo => repo.AddMany(It.IsAny<IEnumerable<FeedbackEntity>>()))
            .Callback<IEnumerable<FeedbackEntity>>(feedbackEntities =>
            {
                for (var i = 0; i < feedbacks.Count; i++)
                {
                    Assert.Equal(feedbacks[i], feedbackEntities.ElementAt(i).Message);
                }
            });
        
        await _clientService.AddFeedbacks(clientId, feedbacks);
    }

    [Fact]
    public async Task AddFeedbacks_ShouldAppendFeedbacks()
    {
        var clientId = 1;
        var feedbacks = new List<string>() { "feedback1", "feedback2" };
        var clientEntity = new ClientEntity() { Id = 1, Feedbacks = new List<FeedbackEntity>() { new FeedbackEntity() { Message = "feedback3" } } };
        
        _clientRepository.Setup(repo => repo.GetById(It.IsAny<int>()))
            .ReturnsAsync(clientEntity);
        
        _feedbackRepository.Setup(repo => repo.AddMany(It.IsAny<IEnumerable<FeedbackEntity>>()))
            .Callback<IEnumerable<FeedbackEntity>>(feedbackEntities =>
            {
                Assert.Equal("feedback3", feedbackEntities.ElementAt(0).Message);
                Assert.Equal(feedbacks[0], feedbackEntities.ElementAt(1).Message);
                Assert.Equal(feedbacks[1], feedbackEntities.ElementAt(2).Message);
            });

        await _clientService.AddFeedbacks(clientId, feedbacks);
    }
    
    [Fact]
    public async Task UpdateSessionDuration_ShouldThrowAnErrorWhenIdIsNotCorrect()
    {
        _clientRepository.Setup(repo => repo.GetById(It.IsAny<int>()))
            .ReturnsAsync((ClientEntity) null);
        
        await Assert.ThrowsAsync<AppException>(() => _clientService.UpdateSessionDuration(2));
    }
    
    [Fact]
    public async Task UpdateSessionDuration_ShouldPassSessionDurationToRepository()
    {
        var clientEntity = new ClientEntity() { Id = 1, VisitTime = DateTime.Now.AddMinutes(-5).ToUniversalTime() };
        
        _clientRepository.Setup(repo => repo.GetById(It.IsAny<int>()))
            .ReturnsAsync(clientEntity);
        
        _clientRepository.Setup(repo => repo.Update(It.IsAny<ClientEntity>()))
            .Callback<ClientEntity>(entity =>
            {
                Assert.Equal(TimeSpan.FromMinutes(5).TotalMinutes, (int)entity.TimeOnSite.TotalMinutes);
            });
        
        await _clientService.UpdateSessionDuration(1);
    }
    
    [Fact]
    public async Task GetFile_ShouldThrowAnErrorWhenIdIsNotCorrect()
    {
        _clientRepository.Setup(repo => repo.GetById(It.IsAny<int>()))
            .ReturnsAsync((ClientEntity) null);
        
        await Assert.ThrowsAsync<AppException>(() => _clientService.GetFileStream(2, OperatingSystem.Windows));
    }
    
    [Fact]
    public async Task GetFile_ShouldThrowAnErrorWhenFileDoesNotExist()
    {
        var clientEntity = new ClientEntity() { Id = 1 };
        
        _clientRepository.Setup(repo => repo.GetById(It.IsAny<int>()))
            .ReturnsAsync(clientEntity);
        
        await Assert.ThrowsAsync<AppException>(() => _clientService.GetFileStream(1, OperatingSystem.Windows));
    }
    
    [Fact]
    public async Task GetFile_ShouldPassUpdatedDownloadsToRepository()
    {
        var clientEntity = new ClientEntity() { Id = 1, Downloads = 1 };
        
        _clientRepository.Setup(repo => repo.GetById(It.IsAny<int>()))
            .ReturnsAsync(clientEntity);
        
        _clientRepository.Setup(repo => repo.Update(It.IsAny<ClientEntity>()))
            .Callback<ClientEntity>(entity =>
            {
                Assert.Equal(2, entity.Downloads);
            });
        
        _fileService.Setup(service => service.Exists(It.IsAny<string>())).Returns(true);
        
        await _clientService.GetFileStream(1, OperatingSystem.Windows);
    }
}