using lanstreamer_api_tests.Integration.Abstract;
using lanstreamer_api.Data.Modules.AccessCode;

namespace lanstreamer_api_tests.Integration.Workers;

public class CleanupSchedulerTests : IntegrationTestsBase
{
    [Fact]
    public async Task ShouldRemoveOldAccessRecords()
    {
        Context.Accesses.Add(new AccessEntity()
        {
            Id = 1,
            Code = "123",
            ExpirationDate = new DateTime().AddMinutes(-5).ToUniversalTime()
        });
        Context.Accesses.Add(new AccessEntity()
        {
            Id = 2,
            Code = "456",
            ExpirationDate = new DateTime().AddMinutes(5).ToUniversalTime()
        });
        await Context.SaveChangesAsync();
        
        // TODO
    }
}