using Core.Contexts;
using Core.Models;
using Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

namespace ChitChat.Core.Tests
{
    public class SendMessageTests
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
                .AddTransient(provider => new MessageService(_inMemoryContext));

            _serviceProvider = serviceCollection.BuildServiceProvider();
        }

        [Test]
        public void SendMessage()
        {
            var personService = _serviceProvider.GetService<PersonService>();

            var sender = new Person
            {
                
                Username = "JohnDoe"
            };

            var recivier = new Person
            {
                Username = "JohnDoe"
            };


        }
    }
}