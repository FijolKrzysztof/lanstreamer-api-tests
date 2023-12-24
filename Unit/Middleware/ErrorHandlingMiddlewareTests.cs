using System.Net;
using lanstreamer_api.App.Data.Dto.Responses;
using lanstreamer_api.App.Exceptions;
using lanstreamer_api.App.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;

namespace lanstreamer_api_tests.Unit.Middleware;

public class ErrorHandlingMiddlewareTests
{
    [Fact]
    public async Task AppException_ShouldReturnCorrectErrorResponseFromAppException()
    {
        const HttpStatusCode statusCode = HttpStatusCode.BadRequest;
        const string message = "Test AppException message";
        
        var logger = new Mock<ILogger<ErrorHandlingMiddleware>>();
        var middleware = new ErrorHandlingMiddleware(_ => throw new AppException(statusCode, message), logger.Object);
        var context = new DefaultHttpContext().HttpContext;
        
        context.Response.Body = new MemoryStream();
        
        await middleware.Invoke(context);

        Assert.Equal((int)statusCode, context.Response.StatusCode);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(responseBody)!;
        
        Assert.Equal((int)statusCode, errorResponse.StatusCode);
        Assert.Equal(message, errorResponse.Message);
    }
    
    [Fact]
    public async Task Exception_ShouldReturnCorrectErrorResponseFromException()
    {
        const int expectedStatusCode = StatusCodes.Status500InternalServerError;
        
        var logger = new Mock<ILogger<ErrorHandlingMiddleware>>();
        var middleware = new ErrorHandlingMiddleware(_ => throw new Exception("Test Exception message"), logger.Object);
        var context = new DefaultHttpContext().HttpContext;
        
        context.Response.Body = new MemoryStream();
        
        await middleware.Invoke(context);

        Assert.Equal(expectedStatusCode, context.Response.StatusCode);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(responseBody)!;
        
        Assert.Equal(expectedStatusCode, errorResponse.StatusCode);
        Assert.Equal("Internal server error", errorResponse.Message);
    }
}