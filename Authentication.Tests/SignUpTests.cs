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
    public class SignUpTests
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
        public async Task SignUp()
        {
            var email = "JohnDoe@example.com";
            var password = Guid.NewGuid().ToString();
            var mockEmailConfirmationToken = Guid.NewGuid().ToString();

            _userManagerMock
                .Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock
                .Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<User>()))
                .ReturnsAsync(mockEmailConfirmationToken);

            var authenticateService = _serviceProvider.GetService<AuthenticateService>();

            Assert.DoesNotThrowAsync(
                async () => await authenticateService.SignUpAsync(email, password), "Should not throw any Exception"
            );

            _userManagerMock.Verify(x => x.CreateAsync(
                It.Is<User>(x => x.Email == email && x.UserName == email), password
            ), "Should call CreateAsync");

            await authenticateService.SignUpAsync(email, password);
        }

        [Test]
        public void SignUpWithExistingUser()
        {
            var email = "JohnDoe@example.com";
            var password = Guid.NewGuid().ToString();

            var duplicatedUser = new IdentityError { Code = nameof(IdentityErrorDescriber.DuplicateEmail) };

            _userManagerMock
                .Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(duplicatedUser));

            var authenticateService = _serviceProvider.GetService<AuthenticateService>();

            var exception = Assert.ThrowsAsync<UserSignUpException>(
                async () => await authenticateService.SignUpAsync(email, password),
                "Should have throw 'UserSignUpException'"
            );

            Assert.That(exception.Message == "Email already registered.", "Exception message should be 'Email already registered.'");
            Assert.That(exception.Error.Code == duplicatedUser.Code, $"Error should be '{nameof(IdentityErrorDescriber.DuplicateEmail)}'");
        }

        [Test]
        public void SignUpWithInvalidPassword()
        {
            var email = "JohnDoe@example.com";
            var password = Guid.NewGuid().ToString();

            var invalidPassword = new IdentityError { Code = nameof(IdentityErrorDescriber.PasswordTooShort) };

            _userManagerMock
                .Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(invalidPassword));

            var authenticateService = _serviceProvider.GetService<AuthenticateService>();

            var exception = Assert.ThrowsAsync<UserSignUpException>(
                async () => await authenticateService.SignUpAsync(email, password),
                "Should have throw 'UserCreationException'"
            );

            Assert.That(exception.Message == "Password needs to be at least 6 character long.", "Exception message should be 'Password needs to be at least 6 character long.'");
            Assert.That(exception.Error.Code == invalidPassword.Code, $"Should have throw Exception with code '{invalidPassword.Code}'");
        }
    }
}
