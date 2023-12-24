using lanstreamer_api.App.Client;
using lanstreamer_api.App.Data.Models;
using lanstreamer_api.Data.Modules.IpLocation;
using lanstreamer_api.Entities;
using lanstreamer_api.Models;

namespace lanstreamer_api_tests.Unit.Modules.Client;

public class ClientConverterTests
{
    private readonly ClientConverter _clientConverter;

    public ClientConverterTests()
    {
        _clientConverter = new ClientConverter();
    }

    [Fact]
    public void ShouldConvertClientDtoToClient()
    {
        var clientDto = new ClientDto()
        {
            Id = 1,
            ReferrerWebsite = "https://google.com"
        };

        var client = _clientConverter.Convert<lanstreamer_api.App.Data.Models.Client>(clientDto);

        Assert.Equal(clientDto.Id, client.Id);
        Assert.Equal(clientDto.ReferrerWebsite, client.ReferrerWebsite);
    }

    [Fact]
    public void ShouldConvertClientToClientDto()
    {
        var client = new lanstreamer_api.App.Data.Models.Client()
        {
            Id = 1,
            ReferrerWebsite = "https://google.com",
        };

        var clientDto = _clientConverter.Convert<ClientDto>(client);

        Assert.Equal(client.Id, clientDto.Id);
        Assert.Equal(client.ReferrerWebsite, clientDto.ReferrerWebsite);
    }

    [Fact]
    public void ShouldConvertClientToClientEntity()
    {
        var client = new lanstreamer_api.App.Data.Models.Client()
        {
            Id = 1,
            Feedbacks = new List<string> { "Good", "Great" },
            ReferrerWebsite = "www.example.com",
            VisitTime = DateTime.Now,
            TimeOnSite = TimeSpan.FromMinutes(30),
            OperatingSystem = "Windows",
            Language = "English",
            Downloads = 5,
            IpLocation = new IpLocation()
            {
                Ip = "192.168.1.1",
                City = "New York",
                Region = "NY",
                Country = "USA",
                Postal = "10001",
                Timezone = "EST",
                Loc = "40.7128,-74.0060",
            },
        };

        var clientEntity = _clientConverter.Convert<ClientEntity>(client);

        Assert.Equal(client.Id, clientEntity.Id);
        Assert.Equal(client.ReferrerWebsite, clientEntity.ReferrerWebsite);
        Assert.Equal(client.VisitTime, clientEntity.VisitTime);
        Assert.Equal(client.TimeOnSite, clientEntity.TimeOnSite);
        Assert.Equal(client.OperatingSystem.ToString(), clientEntity.OperatingSystem);
        Assert.Equal(client.Language, clientEntity.Language);
        Assert.Equal(client.Downloads, clientEntity.Downloads);

        Assert.NotNull(clientEntity.IpLocation);

        Assert.Equal(client.IpLocation.Ip, clientEntity.IpLocation.Ip);
        Assert.Equal(client.IpLocation.City, clientEntity.IpLocation.City);
        Assert.Equal(client.IpLocation.Region, clientEntity.IpLocation.Region);
        Assert.Equal(client.IpLocation.Country, clientEntity.IpLocation.Country);
        Assert.Equal(client.IpLocation.Postal, clientEntity.IpLocation.Postal);
        Assert.Equal(client.IpLocation.Timezone, clientEntity.IpLocation.Timezone);
        Assert.Equal(client.IpLocation.Loc, clientEntity.IpLocation.Loc);

        for (var i = 0; i < client.Feedbacks.Count; i++)
        {
            Assert.Equal(client.Id, clientEntity.Feedbacks[i].ClientId);
            Assert.Equal(client.Feedbacks[i], clientEntity.Feedbacks[i].Message);
        }
    }

    [Fact]
    public void ShouldConvertClientEntityToClient()
    {
        var clientEntity = new ClientEntity()
        {
            Id = 1,
            ReferrerWebsite = "www.example.com",
            VisitTime = DateTime.Now,
            TimeOnSite = TimeSpan.FromMinutes(30),
            OperatingSystem = "Windows",
            Language = "English",
            Downloads = 5,
            IpLocation = new IpLocationEntity()
            {
                Ip = "192.168.1.1",
                City = "New York",
                Region = "NY",
                Country = "USA",
                Postal = "10001",
                Timezone = "EST",
                Loc = "40.7128,-74.0060",
            },
            Feedbacks = new List<FeedbackEntity>()
            {
                new FeedbackEntity()
                {
                    Message = "Good",
                },
                new FeedbackEntity()
                {
                    Message = "Great",
                },
            },
        };

        var client = _clientConverter.Convert<lanstreamer_api.App.Data.Models.Client>(clientEntity);

        Assert.Equal(clientEntity.Id, client.Id);
        Assert.Equal(clientEntity.ReferrerWebsite, client.ReferrerWebsite);
        Assert.Equal(clientEntity.VisitTime, client.VisitTime);
        Assert.Equal(clientEntity.TimeOnSite, client.TimeOnSite);
        Assert.Equal(clientEntity.OperatingSystem, client.OperatingSystem.ToString());
        Assert.Equal(clientEntity.Language, client.Language);
        Assert.Equal(clientEntity.Downloads, client.Downloads);

        Assert.NotNull(client.IpLocation);

        Assert.Equal(clientEntity.IpLocation.Ip, client.IpLocation.Ip);
        Assert.Equal(clientEntity.IpLocation.City, client.IpLocation.City);
        Assert.Equal(clientEntity.IpLocation.Region, client.IpLocation.Region);
        Assert.Equal(clientEntity.IpLocation.Country, client.IpLocation.Country);
        Assert.Equal(clientEntity.IpLocation.Postal, client.IpLocation.Postal);
        Assert.Equal(clientEntity.IpLocation.Timezone, client.IpLocation.Timezone);
        Assert.Equal(clientEntity.IpLocation.Loc, client.IpLocation.Loc);

        for (var i = 0; i < clientEntity.Feedbacks.Count; i++)
        {
            Assert.Equal(clientEntity.Feedbacks[i].Message, client.Feedbacks[i]);
        }
    }

    [Fact]
    public void ShouldConvertClientEntityToClientDto()
    {
        var clientEntity = new ClientEntity()
        {
            Id = 1,
            ReferrerWebsite = "www.example.com",
            Feedbacks = new List<FeedbackEntity>()
            {
                new()
                {
                    Message = "Good",
                },
                new()
                {
                    Message = "Great",
                },
            },
        };
        
        var clientDto = _clientConverter.ChainConvert<lanstreamer_api.App.Data.Models.Client>(clientEntity).To<ClientDto>();
        
        Assert.Equal(clientEntity.Id, clientDto.Id);
        Assert.Equal(clientEntity.ReferrerWebsite, clientDto.ReferrerWebsite);
    }
    
    [Fact]
    public void ShouldConvertClientDtoToClientEntity()
    {
        var clientDto = new ClientDto()
        {
            Id = 1,
            ReferrerWebsite = "www.example.com",
        };
        
        var clientEntity = _clientConverter.ChainConvert<lanstreamer_api.App.Data.Models.Client>(clientDto).To<ClientEntity>();
        
        Assert.Equal(clientDto.Id, clientEntity.Id);
        Assert.Equal(clientDto.ReferrerWebsite, clientEntity.ReferrerWebsite);
    }
}