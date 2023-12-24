using lanstreamer_api.App.Data.Models;
using lanstreamer_api.App.Modules;
using lanstreamer_api.Data.Modules.IpLocation;
using lanstreamer_api.Data.Modules.User;
using lanstreamer_api.Models;

namespace lanstreamer_api_tests.Unit.Modules.User;

public class UserConverterTests
{
    private readonly UserConverter _userConverter;

    public UserConverterTests()
    {
        _userConverter = new UserConverter();
    }

    [Fact]
    public void ShouldConvertUserDtoToUser()
    {
        var userDto = new UserDto()
        {
            Id = 1,
            AccessCode = "1234",
        };

        var user = _userConverter.Convert<lanstreamer_api.App.Data.Models.User>(userDto);

        Assert.Equal(userDto.Id, user.Id);
        Assert.Equal(userDto.AccessCode, user.AccessCode);
    }

    [Fact]
    public void ShouldConvertUserToUserDto()
    {
        var user = new lanstreamer_api.App.Data.Models.User()
        {
            Id = 1,
            AccessCode = "1234",
        };

        var userDto = _userConverter.Convert<UserDto>(user);

        Assert.Equal(user.Id, userDto.Id);
        Assert.Equal(user.AccessCode, userDto.AccessCode);
    }

    [Fact]
    public void ShouldConvertUserToUserEntity()
    {
        var user = new lanstreamer_api.App.Data.Models.User()
        {
            Id = 1,
            AccessCode = "1234",
            Email = "email",
            LastLogin = new DateTime(),
            AppVersion = 1.0f,
            GoogleId = "googleId",
            IpLocation = new IpLocation()
            {
                Ip = "192.168.1.1",
                City = "New York",
                Region = "NY",
                Country = "USA",
                Postal = "10001",
                Timezone = "EST",
                Loc = "40.7128,-74.0060",
            },
        };

        var userEntity = _userConverter.Convert<UserEntity>(user);

        Assert.Equal(user.Id, userEntity.Id);
        Assert.Equal(user.AccessCode, userEntity.AccessCode);
        Assert.Equal(user.Email, userEntity.Email);
        Assert.Equal(user.LastLogin, userEntity.LastLogin);
        Assert.Equal(user.AppVersion, userEntity.AppVersion);
        Assert.Equal(user.GoogleId, userEntity.GoogleId);

        Assert.NotNull(userEntity.IpLocation);

        Assert.Equal(user.IpLocation.Ip, userEntity.IpLocation.Ip);
        Assert.Equal(user.IpLocation.City, userEntity.IpLocation.City);
        Assert.Equal(user.IpLocation.Region, userEntity.IpLocation.Region);
        Assert.Equal(user.IpLocation.Country, userEntity.IpLocation.Country);
        Assert.Equal(user.IpLocation.Postal, userEntity.IpLocation.Postal);
        Assert.Equal(user.IpLocation.Timezone, userEntity.IpLocation.Timezone);
        Assert.Equal(user.IpLocation.Loc, userEntity.IpLocation.Loc);
    }

    [Fact]
    public void ShouldConvertUserEntityToUser()
    {
        var userEntity = new UserEntity()
        {
            Id = 1,
            AccessCode = "1234",
            Email = "email",
            LastLogin = new DateTime(),
            AppVersion = 1.0f,
            GoogleId = "googleId",
            IpLocation = new IpLocationEntity()
            {
                Ip = "192.168.1.1",
                City = "New York",
                Region = "NY",
                Country = "USA",
                Postal = "10001",
                Timezone = "EST",
                Loc = "40.7128,-74.0060",
            },
        };
        
        var user = _userConverter.Convert<lanstreamer_api.App.Data.Models.User>(userEntity);
        
        Assert.Equal(userEntity.Id, user.Id);
        Assert.Equal(userEntity.AccessCode, user.AccessCode);
        Assert.Equal(userEntity.Email, user.Email);
        Assert.Equal(userEntity.LastLogin, user.LastLogin);
        Assert.Equal(userEntity.AppVersion, user.AppVersion);
        Assert.Equal(userEntity.GoogleId, user.GoogleId);
        
        Assert.NotNull(user.IpLocation);
        
        Assert.Equal(userEntity.IpLocation.Ip, user.IpLocation.Ip);
        Assert.Equal(userEntity.IpLocation.City, user.IpLocation.City);
        Assert.Equal(userEntity.IpLocation.Region, user.IpLocation.Region);
        Assert.Equal(userEntity.IpLocation.Country, user.IpLocation.Country);
        Assert.Equal(userEntity.IpLocation.Postal, user.IpLocation.Postal);
        Assert.Equal(userEntity.IpLocation.Timezone, user.IpLocation.Timezone);
        Assert.Equal(userEntity.IpLocation.Loc, user.IpLocation.Loc);
    }

    [Fact]
    public void ShouldConvertUserEntityToUserDto()
    {
        var userEntity = new UserEntity()
        {
            Id = 1,
            AccessCode = "1234",
        };
        
        var userDto = _userConverter.ChainConvert<lanstreamer_api.App.Data.Models.User>(userEntity).To<UserDto>();
        
        Assert.Equal(userEntity.Id, userDto.Id);
        Assert.Equal(userEntity.AccessCode, userDto.AccessCode);
    }
    
    [Fact]
    public void ShouldConvertUserDtoToUserEntity()
    {
        var userDto = new UserDto()
        {
            Id = 1,
            AccessCode = "1234",
        };
        
        var userEntity = _userConverter.ChainConvert<lanstreamer_api.App.Data.Models.User>(userDto).To<UserEntity>();
        
        Assert.Equal(userDto.Id, userEntity.Id);
        Assert.Equal(userDto.AccessCode, userEntity.AccessCode);
    }
}