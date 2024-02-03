using Google.Apis.Auth;
using lanstreamer_api;
using lanstreamer_api.App.Modules.Shared.GoogleAuthenticationService;
using lanstreamer_api.Data.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace lanstreamer_api_tests.Integration.Abstract;

public abstract class IntegrationTestsBase : IDisposable
{
    protected readonly TestServer Server;
    protected readonly HttpClient Client;
    protected readonly ApplicationDbContext Context;
    protected const string CorrectToken = "correct-token";

    protected IntegrationTestsBase()
    {
        var configuration = new Mock<IConfiguration>();
        configuration.Setup(x => x.GetSection("ConnectionStrings")["Schema"]).Returns("test");
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase_" + Guid.NewGuid())
            .Options;

        var googleServiceMock = new Mock<IGoogleAuthenticationService>();
        googleServiceMock.Setup(x => x.VerifyGoogleToken(CorrectToken))
            .ReturnsAsync(new GoogleJsonWebSignature.Payload()
            {
                Email = "email",
                Subject = "subject/id",
                Name = "name"
            }); 
        googleServiceMock.Setup(x => x.VerifyGoogleToken(It.Is<string>(s => s != CorrectToken))).Throws<Exception>();
        
        Server = new TestServer(new WebHostBuilder()
            .UseStartup<Startup>()
            .ConfigureTestServices(services =>
            {
                services.AddScoped<IGoogleAuthenticationService>(_ => googleServiceMock.Object);
                services.AddScoped<ApplicationDbContext>(_ => new ApplicationDbContext(options, configuration.Object));
            }));

        Client = Server.CreateClient();

        Context = Server.Host.Services.GetRequiredService<ApplicationDbContext>();
    }
    
    public void Dispose()
    {
        Context.Database.EnsureDeleted();
        Context.Dispose();
        Client.Dispose();
        Server.Dispose();
    }
}