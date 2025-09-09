using System;
using System.Threading.Tasks;
using FluentAssertions;
using Obfuscator.Interfaces;
using Xunit;

namespace Obfuscator.Tests.Interfaces
{
    public class IObfuscatorServiceTests
    {
        [Fact]
        public void IObfuscatorService_ShouldHaveCorrectMethodSignature()
        {
            // Arrange
            var interfaceType = typeof(IObfuscatorService);
            
            // Act
            var method = interfaceType.GetMethod("SanitizeSensitiveData");
            
            // Assert
            method.Should().NotBeNull();
            method!.IsGenericMethodDefinition.Should().BeTrue();
            method.GetGenericArguments().Should().HaveCount(1);
            method.ReturnType.Should().Be(typeof(string));
            
            var parameters = method.GetParameters();
            parameters.Should().HaveCount(1);
            parameters[0].Name.Should().Be("input");
        }

        [Fact]
        public void IObfuscatorService_ShouldBeInterface()
        {
            // Arrange & Act
            var interfaceType = typeof(IObfuscatorService);
            
            // Assert
            interfaceType.IsInterface.Should().BeTrue();
        }

        [Fact]
        public void IObfuscatorService_ShouldBeInCorrectNamespace()
        {
            // Arrange & Act
            var interfaceType = typeof(IObfuscatorService);
            
            // Assert
            interfaceType.Namespace.Should().Be("Obfuscator.Interfaces");
        }

        [Fact]
        public void IObfuscatorService_ShouldBePublic()
        {
            // Arrange & Act
            var interfaceType = typeof(IObfuscatorService);
            
            // Assert
            interfaceType.IsPublic.Should().BeTrue();
        }

        [Fact]
        public void IObfuscatorService_ShouldNotHaveBaseInterfaces()
        {
            // Arrange & Act
            var interfaceType = typeof(IObfuscatorService);
            var baseInterfaces = interfaceType.GetInterfaces();
            
            // Assert
            baseInterfaces.Should().BeEmpty();
        }
    }
}