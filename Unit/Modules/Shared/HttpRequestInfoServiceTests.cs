using System.Security.Claims;
using lanstreamer_api_tests.Utills;
using lanstreamer_api.services;
using Microsoft.Extensions.Logging;
using Moq;

namespace lanstreamer_api_tests.Unit.Modules.Shared;

public class HttpRequestInfoServiceTests
{
    private readonly HttpRequestInfoService _httpRequestInfoService;

    public HttpRequestInfoServiceTests()
    {
        var logger = new Mock<ILogger<HttpRequestInfoService>>();

        _httpRequestInfoService = new HttpRequestInfoService(logger.Object);
    }

    [Fact]
    public void ShouldReturnCorrectRoles()
    {
        var roles = new List<string>() { "role1", "role2" };
        var httpContext = TestObjectFactory.GetHttpContextAccessor(roles: roles).HttpContext!;
        
        Assert.Equal(roles, _httpRequestInfoService.GetRoles(httpContext));
    }

    [Fact]
    public void ShouldReturnCorrectEmail()
    {
        var email = "email";
        var httpContext = TestObjectFactory.GetHttpContextAccessor(claims: new Dictionary<string, string>()
        {
            { ClaimTypes.Email, email }
        }).HttpContext!;

        Assert.Equal(email, _httpRequestInfoService.GetEmail(httpContext));
    }

    [Fact]
    public void ShouldReturnCorrectIdentity()
    {
        var identity = "identity";
        var httpContext = TestObjectFactory.GetHttpContextAccessor(claims: new Dictionary<string, string>()
        {
            { ClaimTypes.NameIdentifier, identity }
        }).HttpContext!;

        Assert.Equal(identity, _httpRequestInfoService.GetIdentity(httpContext));
    }

    [Fact]
    public void ShouldReturnCorrectIpAddress()
    {
        var xForwardedFor = "192.158.1.38, 2001:db8:85a3:8d3:1319:8a2e:370:7348";
        var ip = _httpRequestInfoService.GetIpAddress(TestObjectFactory
            .GetHttpContextAccessor(xForwardedFor: xForwardedFor).HttpContext!);

        Assert.Equal("192.158.1.38", ip);
    }

    [Fact]
    public void ShouldReturnCorrectOs()
    {
        var userAgent = "windows";
        var windowsContext = TestObjectFactory.GetHttpContextAccessor(userAgent: userAgent).HttpContext!;

        Assert.Equal(userAgent, _httpRequestInfoService.GetOs(windowsContext));
    }

    [Fact]
    public void ShouldReturnCorrectDefaultLanguage()
    {
        var acceptLanguage = "en-US,en;q=0.9,he;q=0.8";
        var httpContext = TestObjectFactory.GetHttpContextAccessor(acceptLanguage: acceptLanguage).HttpContext!;

        Assert.Equal("en", _httpRequestInfoService.GetDefaultLanguage(httpContext));
    }

    [Fact]
    public async Task ShouldReturnCorrectIpLocation()
    {
        var ip = "192.158.1.38";
        var ipLocation = await _httpRequestInfoService.GetIpLocation(ip);

        Assert.Equal(ip, ipLocation.Ip);
        Assert.Equal("US", ipLocation.Country);
        Assert.Equal("California", ipLocation.Region);
        Assert.Equal("Palo Alto", ipLocation.City);
        Assert.Equal("94304", ipLocation.Postal);
        Assert.Equal("America/Los_Angeles", ipLocation.Timezone);
        Assert.Equal("37.4334,-122.1842", ipLocation.Loc);
    }
}