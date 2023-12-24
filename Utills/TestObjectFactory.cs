using System.Security.Claims;
using lanstreamer_api.Data.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace lanstreamer_api_tests.Utills;

public static class TestObjectFactory
{
    public static ApplicationDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;

        return new ApplicationDbContext(options);
    }

    public static HttpContextAccessor GetHttpContextAccessor(
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
}