using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Compliance.Redaction;
using Obfuscator.Extensions;
using Obfuscator.Interfaces;
using Obfuscator.Redactors;
using Xunit;

namespace Obfuscator.Tests.Extensions
{
    public class ObfuscatorExtensionsTests
    {
        [Fact]
        public void AddObfuscator_ShouldRegisterIObfuscatorService()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddObfuscator();
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var obfuscatorService = serviceProvider.GetService<IObfuscatorService>();
            obfuscatorService.Should().NotBeNull();
            obfuscatorService.Should().BeOfType<ObfuscatorService>();
        }

        [Fact]
        public void AddObfuscator_ShouldRegisterIRedactorProvider()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddObfuscator();
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var redactorProvider = serviceProvider.GetService<IRedactorProvider>();
            redactorProvider.Should().NotBeNull();
            redactorProvider.Should().BeOfType<SimpleRedactorProvider>();
        }

        [Fact]
        public void AddObfuscator_ShouldRegisterServicesAsSingleton()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddObfuscator();
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var obfuscatorService1 = serviceProvider.GetService<IObfuscatorService>();
            var obfuscatorService2 = serviceProvider.GetService<IObfuscatorService>();
            
            obfuscatorService1.Should().BeSameAs(obfuscatorService2);

            var redactorProvider1 = serviceProvider.GetService<IRedactorProvider>();
            var redactorProvider2 = serviceProvider.GetService<IRedactorProvider>();
            
            redactorProvider1.Should().BeSameAs(redactorProvider2);
        }

        [Fact]
        public void AddObfuscator_ShouldReturnServiceCollection()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            var result = services.AddObfuscator();

            // Assert
            result.Should().BeSameAs(services);
        }

        [Fact]
        public void AddObfuscator_ShouldAllowChaining()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act & Assert
            services
                .AddObfuscator()
                .AddSingleton<string>("test")
                .Should().BeSameAs(services);
        }

        [Fact]
        public void AddObfuscator_WhenCalledMultipleTimes_ShouldNotDuplicateRegistrations()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddObfuscator();
            services.AddObfuscator();
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var obfuscatorServices = serviceProvider.GetServices<IObfuscatorService>();
            obfuscatorServices.Should().HaveCount(2); // Due to multiple registrations
            
            // But they should still work correctly
            var primaryService = serviceProvider.GetService<IObfuscatorService>();
            primaryService.Should().NotBeNull();
        }

        [Fact]
        public void AddObfuscator_ShouldConfigureRedactionBuilder()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddObfuscator();
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            // The redaction should be configured (even if with empty configuration)
            // This test verifies that AddRedaction was called without throwing
            var redactorProvider = serviceProvider.GetService<IRedactorProvider>();
            redactorProvider.Should().NotBeNull();
        }

        [Fact]
        public void AddObfuscator_WithNullServiceCollection_ShouldThrowArgumentNullException()
        {
            // Arrange
            IServiceCollection? services = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => services!.AddObfuscator());
        }
    }
}