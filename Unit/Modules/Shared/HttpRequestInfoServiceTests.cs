using System.Security.Claims;
using lanstreamer_api.services;
using Microsoft.AspNetCore.Http;
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
    
    private HttpContextAccessor GetHttpContextAccessor(
        string? acceptLanguage = null,
        string? userAgent = null,
        string? xForwardedFor = null,
        List<string>? roles = null,
        Dictionary<string, string>? claims = null)
    {
        var headers = new HeaderDictionary();
        if (!string.IsNullOrEmpty(acceptLanguage))
        {
            headers.Add("Accept-Language", acceptLanguage);
        }

        if (!string.IsNullOrEmpty(userAgent))
        {
            headers.Add("User-Agent", userAgent);
        }

        if (!string.IsNullOrEmpty(xForwardedFor))
        {
            headers.Add("X-Forwarded-For", xForwardedFor);
        }

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.SetupGet(x => x.Request.Headers).Returns(headers);

        var claimsIdentity = new ClaimsIdentity();

        if (roles != null && roles.Any())
        {
            roles.ForEach(role =>
            {
                claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role));
            });
        }

        if (claims != null)
        {
            var otherClaims = claims.Select(c => new Claim(c.Key, c.Value));
            claimsIdentity.AddClaims(otherClaims);
        }

        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        httpContextMock.SetupGet(x => x.User).Returns(claimsPrincipal);

        return new HttpContextAccessor()
        {
            HttpContext = httpContextMock.Object
        };
    }

    [Fact]
    public void ShouldReturnCorrectRoles()
    {
        var roles = new List<string>() { "role1", "role2" };
        var httpContext = GetHttpContextAccessor(roles: roles).HttpContext!;
        
        Assert.Equal(roles, _httpRequestInfoService.GetRoles(httpContext));
    }

    [Fact]
    public void ShouldReturnCorrectEmail()
    {
        const string email = "email";
        var httpContext = GetHttpContextAccessor(claims: new Dictionary<string, string>()
        {
            { ClaimTypes.Email, email }
        }).HttpContext!;

        Assert.Equal(email, _httpRequestInfoService.GetEmail(httpContext));
    }

    [Fact]
    public void ShouldReturnCorrectIdentity()
    {
        const string identity = "identity";
        var httpContext = GetHttpContextAccessor(claims: new Dictionary<string, string>()
        {
            { ClaimTypes.NameIdentifier, identity }
        }).HttpContext!;

        Assert.Equal(identity, _httpRequestInfoService.GetIdentity(httpContext));
    }

    [Fact]
    public void ShouldReturnCorrectIpAddress()
    {
        const string xForwardedFor = "192.158.1.38, 2001:db8:85a3:8d3:1319:8a2e:370:7348";
        var ip = _httpRequestInfoService
            .GetIpAddress(GetHttpContextAccessor(xForwardedFor: xForwardedFor).HttpContext!);

        Assert.Equal("192.158.1.38", ip);
    }

    [Fact]
    public void ShouldReturnCorrectOs()
    {
        const string userAgent = "windows";
        var windowsContext = GetHttpContextAccessor(userAgent: userAgent).HttpContext!;

        Assert.Equal(userAgent, _httpRequestInfoService.GetOs(windowsContext));
    }

    [Fact]
    public void ShouldReturnCorrectDefaultLanguage()
    {
        const string acceptLanguage = "en-US,en;q=0.9,he;q=0.8";
        var httpContext = GetHttpContextAccessor(acceptLanguage: acceptLanguage).HttpContext!;

        Assert.Equal("en", _httpRequestInfoService.GetDefaultLanguage(httpContext));
    }

    [Fact]
    public async Task ShouldReturnCorrectIpLocation()
    {
        const string ip = "192.158.1.38";
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