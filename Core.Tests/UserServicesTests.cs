using System;
using System.Threading.Tasks;
using Core.Contexts;
using Core.Models;
using Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Core.Tests
{
    public class UserServicesTests
    {
        private ServiceProvider _serviceProvider;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<CoreDBContext>()
                .UseInMemoryDatabase(databaseName: "MovieListDatabase")
                .Options;

            var _inMemoryContext = new CoreDBContext(options);

            var serviceCollection = new ServiceCollection()
                .AddTransient(provider => new UserService(_inMemoryContext));

            _serviceProvider = serviceCollection.BuildServiceProvider();
        }

        [Test]
        public async Task CreaterUser()
        {
            var userService = _serviceProvider.GetService<UserService>();
            var user = new User { Username = "John Doe - 1", Email = "johndoe-1@example.com", RegisterDate = DateTime.Now };
            Assert.DoesNotThrowAsync(async () => await userService.CreaterUser(user), "Should not throw any Exception");

            var saved = await userService.Users.AnyAsync(u => u.Username == user.Username && u.Email == user.Email);
            Assert.IsTrue(saved, "User should have been saved.");
        }

        [Test]
        public async Task CreaterUserWithSameUsername()
        {
            var userService = _serviceProvider.GetService<UserService>();
            var user = new User { Username = "John Doe - 2", Email = "johndoe-2@example.com", RegisterDate = DateTime.Now };
            Assert.DoesNotThrowAsync(async () => await userService.CreaterUser(user), "Should not throw any Exception");

            var saved = await userService.Users.AnyAsync(u => u.Username == user.Username && u.Email == user.Email);
            Assert.IsTrue(saved, "User should have been saved.");

            var exception = Assert.ThrowsAsync<Exception>(async () => await userService.CreaterUser(user), "Should have throw exception");
            Assert.That(exception.Message == "Username already taken", "Exception message should be 'Username already taken.'");
        }

        [Test]
        public async Task CreaterUserWithSameEmail()
        {
            var userService = _serviceProvider.GetService<UserService>();
            var user = new User { Username = "John Doe - 3", Email = "johndoe-3@example.com", RegisterDate = DateTime.Now };

            Assert.DoesNotThrowAsync(async () => await userService.CreaterUser(user), "Should not throw any Exception");
            var saved = await userService.Users.AnyAsync(u => u.Username == user.Username && u.Email == user.Email);
            Assert.IsTrue(saved, "User should have been saved.");

            user.Username = "meh";
            var exception = Assert.ThrowsAsync<Exception>(async () => await userService.CreaterUser(user), "Should have throw exception");
            Assert.That(exception.Message == "Email already taken", "Exception message should be 'Email already taken.'");
        }
    }
}
