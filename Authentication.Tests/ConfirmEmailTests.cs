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

namespace kta_core.tests
{
    class ConfirmEmailTests
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
        public void ConfirmEmailTest()
        {
            var user = new User
            {
                UserName = "JohnDoe@example.com",
                Email = "JohnDoe@example.com"
            };

            var mockTokenConfirmEmail = Guid.NewGuid().ToString();

            _userManagerMock
                .Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(x => x.ConfirmEmailAsync(user, mockTokenConfirmEmail))
                .ReturnsAsync(IdentityResult.Success);

            var authenticateService = _serviceProvider.GetService<AuthenticateService>();

            Assert.DoesNotThrowAsync(async () => await authenticateService.ConfirmEmail(user.Email, mockTokenConfirmEmail));

            _userManagerMock.Verify(x => x.ConfirmEmailAsync(user, mockTokenConfirmEmail), "Should have called 'ConfirmEmailAsync'");
        }

        [Test]
        public void ConfirmEmailWithInvalidEmailInTokenTest()
        {
            var user = new User
            {
                UserName = "JohnDoe@example.com",
                Email = "JohnDoe@example.com"
            };

            var mockTokenConfirmEmail = Guid.NewGuid().ToString();

            _userManagerMock
                .Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(default(User));

            var authenticateService = _serviceProvider.GetService<AuthenticateService>();

            var exception = Assert.ThrowsAsync<InvalidUserException>
            (
                async () => await authenticateService.ConfirmEmail(user.Email, mockTokenConfirmEmail),
                "Should throw 'InvalidUserException'"
            );

            Assert.That(exception.Message == "Invalid Token.", "Message should be 'Invalid Token.'");
        }

        [Test]
        public void ConfirmEmailWithInvalidTokenTest()
        {
            var user = new User
            {
                UserName = "JohnDoe@example.com",
                Email = "JohnDoe@example.com"
            };

            var mockTokenConfirmEmail = Guid.NewGuid().ToString();

            _userManagerMock
                .Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(x => x.ConfirmEmailAsync(user, mockTokenConfirmEmail))
                .ReturnsAsync(IdentityResult.Failed());

            var authenticateService = _serviceProvider.GetService<AuthenticateService>();

            var exception = Assert.ThrowsAsync<InvalidTokenException>
            (
                async () => await authenticateService.ConfirmEmail(user.Email, mockTokenConfirmEmail),
                "Should throw 'InvalidTokenException'"
            );

            Assert.That(exception.Message == "Invalid Token.", "Message should be 'Invalid Token.'");
        }
    }
}
