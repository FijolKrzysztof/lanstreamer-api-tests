using lanstreamer_api_tests.Utills;
using lanstreamer_api.App.Modules;
using lanstreamer_api.Data.Modules.User;
using lanstreamer_api.Models;
using lanstreamer_api.services;
using Microsoft.AspNetCore.Http;
using Moq;

namespace lanstreamer_api_tests.Unit.Modules.User;

public class UserServiceTests
{
    private readonly UserService _userService;
    private readonly Mock<IUserRepository> _userRepository;

    public UserServiceTests()
    {
        _userRepository = new Mock<IUserRepository>();
        var httpRequestsInfoService = new Mock<IHttpRequestInfoService>();
        var serverSentEventsService = new Mock<IServerSentEventsService<bool>>();
        var userConverter = new UserConverter();

        _userService = new UserService(userConverter, _userRepository.Object, serverSentEventsService.Object,
            httpRequestsInfoService.Object);
    }

    [Fact]
    public async Task Login_ShouldPassAccessCodeToRepository()
    {
        var userDto = new UserDto() { AccessCode = "1234" };

        _userRepository.Setup(repo => repo.GetByGoogleId(It.IsAny<string>()))
            .ReturnsAsync(new UserEntity());

        _userRepository.Setup(repo => repo.Update(It.IsAny<UserEntity>()))
            .Callback<UserEntity>(UserEntity => { Assert.Equal(userDto.AccessCode, UserEntity.AccessCode); });

        await _userService.Login(userDto, new Mock<HttpContext>().Object);
    }

    [Fact]
    public async Task Login_ShouldPassLastLoginToRepository()
    {
        _userRepository.Setup(repo => repo.GetByGoogleId(It.IsAny<string>()))
            .ReturnsAsync(new UserEntity());

        _userRepository.Setup(repo => repo.Update(It.IsAny<UserEntity>()))
            .Callback<UserEntity>(userEntity => { Assert.Equal(DateTime.Now.Date, userEntity.LastLogin.Date); });

        await _userService.Login(new UserDto(), new Mock<HttpContext>().Object);
    }
}