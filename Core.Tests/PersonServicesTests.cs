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
    public class PersonServicesTests
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
                .AddTransient(provider => new PersonService(_inMemoryContext));

            _serviceProvider = serviceCollection.BuildServiceProvider();
        }

        [Test]
        public async Task CreatePerson()
        {
            var personService = _serviceProvider.GetService<PersonService>();

            var person = new Person
            {
                Username = "John Doe - 1",
                ExternalId = $"{Guid.NewGuid()}",
                CreationDate = DateTime.Now
            };

            Assert.DoesNotThrowAsync
            (
                async () => await personService.CreaterPerson(person),
                "Should not throw any Exception"
            );

            var saved = await personService.Persons.AnyAsync(u => u.Username == person.Username);

            Assert.IsTrue(saved, "Person should have been saved.");
        }

        [Test]
        public void CreaterPersonWithSameUsername()
        {
            var personService = _serviceProvider.GetService<PersonService>();

            var person = new Person
            {
                Username = "John Doe - 2",
                ExternalId = $"{Guid.NewGuid()}",
                CreationDate = DateTime.Now
            };

            Assert.DoesNotThrowAsync
            (
                async () => await personService.CreaterPerson(person),
                "Should not throw any Exception"
            );

            var otherPerson = new Person
            {
                Username = "John Doe - 2",
                ExternalId = $"{Guid.NewGuid()}",
                CreationDate = DateTime.Now
            };

            var exception = Assert.ThrowsAsync<Exception>
            (
                async () => await personService.CreaterPerson(otherPerson),
                "Should have throw exception"
            );

            Assert.That(exception.Message == "Username already taken", "Exception message should be 'Username already taken.'");
        }
    }
}
