using Authentication.Exceptions;
using Authentication.Models;
using Authentication.Models.Settings;
using Authentication.Services;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Moq;
using NUnit.Framework;

using System;
using System.Threading.Tasks;

namespace Authentication.tests
{
    public class PasswordRecoveryTests
    {
        #region Mocks

        private Mock<UserManager<User>> _userManagerMock;

        #endregion

        private ServiceProvider _serviceProvider;

        [SetUp]
        public void Setup()
        {
            var _userStore = Mock.Of<IUserStore<User>>();

            _userManagerMock = new Mock<UserManager<User>>(_userStore, null, null, null, null, null, null, null, null);

            var authenticateServiceSettings = new AuthenticateServiceSettings { Secret = Guid.NewGuid().ToString() };
            var optionsAuthenticateServiceSettings = Options.Create(authenticateServiceSettings);

            var serviceCollection = new ServiceCollection()
                .AddTransient(provider => new AuthenticateService(optionsAuthenticateServiceSettings, _userManagerMock.Object));

            _serviceProvider = serviceCollection.BuildServiceProvider();
        }

        [Test]
        public async Task CreateResetPasswordTokenTest()
        {
            var authenticateService = _serviceProvider.GetService<AuthenticateService>();

            var mockerUser = new User
            {
                UserName = "JohnDoe@example.com",
                Email = "JohnDoe@example.com"
            };

            var mockResetPasswordToken = Guid.NewGuid().ToString();

            _userManagerMock
                .Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(mockerUser);

            _userManagerMock
                .Setup(x => x.GeneratePasswordResetTokenAsync(It.IsAny<User>()))
                .ReturnsAsync(mockResetPasswordToken);

            var resetPasswordToken = default(string);

            try
            {
                resetPasswordToken = await authenticateService.CreateResetPasswordTokenAsync(mockerUser.Email);
            }
            catch { Assert.Fail("Should not throw Exception"); }

            Assert.IsTrue(resetPasswordToken == mockResetPasswordToken, "Token does not match");

            _userManagerMock.Verify(x => x.FindByEmailAsync(mockerUser.Email), $"Should have call FindByEmailAsync with email {mockerUser.Email}");
            _userManagerMock.Verify(x => x.GeneratePasswordResetTokenAsync(
                It.Is<User>(p => p.Email == mockerUser.Email && p.UserName == mockerUser.UserName)
            ), "Should have call GeneratePasswordResetTokenAsync");
        }

        [Test]
        public void CreateResetPasswordTokenWithInvalidEmailTest()
        {
            var email = "JohnDoe@example.com";
            var passwordResetTokenMock = Guid.NewGuid().ToString();

            _userManagerMock
                .Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(default(User));

            var authenticateService = _serviceProvider.GetService<AuthenticateService>();

            var exception = Assert.ThrowsAsync<InvalidUserException>
            (
                async () => await authenticateService.CreateResetPasswordTokenAsync(email),
                "CreateResetPasswordTokenAsync should have thrown InvalidUserException"
            );

            Assert.That(exception.Message == "The provided email are not registered.",
                "Exception message should have been 'The provided email are not registered.'"
            );

            _userManagerMock.Verify(x => x.FindByEmailAsync(email), $"Should call FindByEmailAsync with email {email}");
        }

        [Test]
        public async Task ResetPassword()
        {
            var user = new User
            {
                UserName = "JohnDoe@example.com",
                Email = "JohnDoe@example.com"
            };

            var password = Guid.NewGuid().ToString();
            var mockResetTokenPassword = Guid.NewGuid().ToString();

            _userManagerMock
                .Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(x => x.ResetPasswordAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            var authenticateService = _serviceProvider.GetService<AuthenticateService>();

            try
            {
                await authenticateService.ResetPasswordAsync(user.Email, password, mockResetTokenPassword);
            }
            catch { Assert.Fail("Should not throw Exception"); }

            _userManagerMock.Verify(x => x.FindByEmailAsync(user.Email), $"Should call FindByEmailAsync with {user.Email}");
            _userManagerMock.Verify(x => x.ResetPasswordAsync(
                It.Is<User>(p => p.Email == user.Email && p.UserName == user.UserName),
                It.Is<string>(p => p == mockResetTokenPassword),
                It.Is<string>(p => p == password)
            ),
            "Should call ResetPasswordAsync");
        }

        [Test]
        public async Task ResetPasswordWithInvalidUser()
        {
            var user = new User
            {
                UserName = "JohnDoe@example.com",
                Email = "JohnDoe@example.com"
            };

            var password = Guid.NewGuid().ToString();
            var mockResetTokenPassword = Guid.NewGuid().ToString();

            _userManagerMock
                .Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(default(User));

            var authenticateService = _serviceProvider.GetService<AuthenticateService>();

            try
            {
                await authenticateService.ResetPasswordAsync(user.Email, password, mockResetTokenPassword);
            }
            catch (InvalidUserException e)
            {
                Assert.That(e.Message == "Invalid Token.");
            }

            _userManagerMock.Verify(x => x.FindByEmailAsync(user.Email), $"Should call FindByEmailAsync with {user.Email}");
        }

        [Test]
        public async Task ResetPasswordWithInvalidPassword()
        {
            var authenticateService = _serviceProvider.GetService<AuthenticateService>();

            var user = new User
            {
                UserName = "JohnDoe@example.com",
                Email = "JohnDoe@example.com"
            };

            var password = Guid.NewGuid().ToString();
            var mockResetTokenPassword = Guid.NewGuid().ToString();
            var mockInvalidPassword = new IdentityError { Code = nameof(IdentityErrorDescriber.PasswordTooShort) };

            _userManagerMock
                .Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(x => x.ResetPasswordAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(mockInvalidPassword));

            try
            {
                await authenticateService.ResetPasswordAsync(user.Email, password, mockResetTokenPassword);
            }
            catch (InvalidTokenException e)
            {
                Assert.That(e.Error.Code == mockInvalidPassword.Code, $"Error code should be {mockInvalidPassword.Code}");
                Assert.That(e.Message == "Password must be 6 character long.", "Message should be 'Password must be 6 character long.'");
            }

            _userManagerMock.Verify(x => x.FindByEmailAsync(user.Email), $"Should call FindByEmailAsync with {user.Email}");
            _userManagerMock.Verify(x => x.ResetPasswordAsync(
                It.Is<User>(p => p.Email == user.Email && p.UserName == user.UserName),
                It.Is<string>(p => p == mockResetTokenPassword),
                It.Is<string>(p => p == password)
            ),
            "Should call ResetPasswordAsync");
        }

        [Test]
        public async Task ResetPasswordWithInvalidToken()
        {
            var authenticateService = _serviceProvider.GetService<AuthenticateService>();

            var user = new User
            {
                UserName = "JohnDoe@example.com",
                Email = "JohnDoe@example.com"
            };

            var password = Guid.NewGuid().ToString();
            var mockResetTokenPassword = Guid.NewGuid().ToString();
            var mockInvalidPassword = new IdentityError { Code = nameof(IdentityErrorDescriber.InvalidToken) };

            _userManagerMock
                .Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(x => x.ResetPasswordAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(mockInvalidPassword));

            try
            {
                await authenticateService.ResetPasswordAsync(user.Email, password, mockResetTokenPassword);
            }
            catch (InvalidTokenException e)
            {
                Assert.That(e.Error.Code == mockInvalidPassword.Code, $"Error code should be {mockInvalidPassword.Code}");
                Assert.That(e.Message == "Invalid Token.", "Message should be 'Invalid Token.'");
            }

            _userManagerMock.Verify(x => x.FindByEmailAsync(user.Email), $"Should call FindByEmailAsync with {user.Email}");
            _userManagerMock.Verify(x => x.ResetPasswordAsync(
                It.Is<User>(p => p.Email == user.Email && p.UserName == user.UserName),
                It.Is<string>(p => p == mockResetTokenPassword),
                It.Is<string>(p => p == password)
            ),
            "Should call ResetPasswordAsync");
        }
    }
}