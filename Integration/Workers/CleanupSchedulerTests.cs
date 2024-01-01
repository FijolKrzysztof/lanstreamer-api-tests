using lanstreamer_api_tests.Integration.Abstract;

namespace lanstreamer_api_tests.Integration.Workers;

public class CleanupSchedulerTests : IntegrationTestsBase
{
    [Fact]
    public async Task ShouldRemoveOldAccessRecords()
    {
        // _context.Accesses.Add(new AccessEntity()
        // {
        //     Id = 1,
        //     Code = "123",
        //     ExpirationDate = new DateTime().AddMinutes(-5).ToUniversalTime()
        // });
        // _context.Accesses.Add(new AccessEntity()
        // {
        //     Id = 2,
        //     Code = "456",
        //     ExpirationDate = new DateTime().AddMinutes(5).ToUniversalTime()
        // });
        // await _context.SaveChangesAsync();
        
        // TODO
    }
}