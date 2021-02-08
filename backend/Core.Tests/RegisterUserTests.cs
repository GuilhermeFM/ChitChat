using Core.Models;
using Core.Services;

using Microsoft.Extensions.DependencyInjection;

using Moq;
using NUnit.Framework;

namespace ChitChat.Core.Tests
{
    public class RegisterUserTests
    {
        private ServiceProvider _serviceProvider;

        [SetUp]
        public void Setup()
        {
            var _userStore = Mock.Of<User>();

            var serviceCollection = new ServiceCollection()
                .AddTransient(provider => new UserService());

            _serviceProvider = serviceCollection.BuildServiceProvider();
        }

        [Test]
        public void RegisterUser()
        {
        }

        [Test]
        public void RegisterUserWithInvalidUsernameField()
        {
        }

        [Test]
        public void RegisterUserWithInvalidPasswordField()
        {
        }

        [Test]
        public void RegisterUserThatAlreadyExists()
        {
        }
    }
}
