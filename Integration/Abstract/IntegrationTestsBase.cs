using Google.Apis.Auth;
using lanstreamer_api;
using lanstreamer_api.App.Modules.Shared.GoogleAuthenticationService;
using lanstreamer_api.Data.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace lanstreamer_api_tests.Integration.Abstract;

public abstract class IntegrationTestsBase : IDisposable
{
    protected readonly TestServer _server;
    protected readonly HttpClient _client;
    protected readonly ApplicationDbContext _context;

    protected IntegrationTestsBase()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase_" + Guid.NewGuid())
            .Options;

        var googleServiceMock = new Mock<IGoogleAuthenticationService>();
        googleServiceMock.Setup(x => x.VerifyGoogleToken("correct-token"))
            .ReturnsAsync(new GoogleJsonWebSignature.Payload()
            {
                Email = "email",
                Subject = "subject/id",
                Name = "name"
            }); 
        googleServiceMock.Setup(x => x.VerifyGoogleToken(It.Is<string>(s => s != "correct-token"))).Throws<Exception>();
        
        _server = new TestServer(new WebHostBuilder()
            .UseStartup<Startup>()
            .ConfigureTestServices(services =>
            {
                services.AddScoped<IGoogleAuthenticationService>(_ => googleServiceMock.Object);
                services.AddScoped<ApplicationDbContext>(_ => new ApplicationDbContext(options));
            }));

        _client = _server.CreateClient();

        _context = _server.Host.Services.GetRequiredService<ApplicationDbContext>();
    }
    
    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        _client.Dispose();
        _server.Dispose();
    }
}