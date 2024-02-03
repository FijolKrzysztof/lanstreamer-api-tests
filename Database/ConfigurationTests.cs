using lanstreamer_api.App.Data.Models.Enums;
using lanstreamer_api.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace lanstreamer_api_tests.Database;

public class ConfigurationTests
{
    private readonly IEnumerable<IConfiguration> _configurations;

    public ConfigurationTests()
    {
        var devConfiguration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Development.json")
            .Build();
        
        var prodConfiguration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        _configurations = new List<IConfiguration>()
        {
            devConfiguration,
            prodConfiguration,
        };
    }
    
    [Fact]
    public async Task AllConfigurations_ShouldBeSeeded()
    {
        foreach (var configuration in _configurations)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseNpgsql(configuration.GetConnectionString("Database"))
                .Options;
        
            await using var context = new ApplicationDbContext(options, configuration);
            var configurations = await context.Configurations.ToListAsync();
        
            Assert.NotEmpty(configurations);
        
            var configurationKeys = Enum.GetValues(typeof(ConfigurationKey))
                .Cast<ConfigurationKey>()
                .Select(key => key.ToString());
        
            var missingConfigurations = configurationKeys.Except(configurations.Select(x => x.Key));

            foreach (var missingConfiguration in missingConfigurations)
            {
                Assert.Null(missingConfiguration);
            }
        }
    }
}
