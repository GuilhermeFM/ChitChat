using Core.Services;

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
            var serviceCollection = new ServiceCollection()
                .AddTransient(provider => new MessageService());

            _serviceProvider = serviceCollection.BuildServiceProvider();
        }

        [Test]
        public void SendMessage()
        {

        }
    }
}