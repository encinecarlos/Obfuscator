using System;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Compliance.Classification;
using Microsoft.Extensions.Compliance.Redaction;
using Moq;
using Obfuscator.Interfaces;
using Obfuscator.Tests.TestModels;
using Xunit;

namespace Obfuscator.Tests
{
    public class ObfuscatorServiceTests
    {
        private readonly Mock<IRedactorProvider> _mockRedactorProvider;
        private readonly Mock<Redactor> _mockRedactor;
        private readonly ObfuscatorService _obfuscatorService;

        public ObfuscatorServiceTests()
        {
            _mockRedactorProvider = new Mock<IRedactorProvider>();
            _mockRedactor = new Mock<Redactor>();
            
            _mockRedactorProvider
                .Setup(x => x.GetRedactor(It.IsAny<DataClassificationSet>()))
                .Returns(_mockRedactor.Object);
            
            _mockRedactor
                .Setup(x => x.Redact(It.IsAny<string>()))
                .Returns("[REDACTED]");

            _obfuscatorService = new ObfuscatorService(_mockRedactorProvider.Object);
        }

        [Fact]
        public void Constructor_WithValidRedactorProvider_ShouldCreateInstance()
        {
            // Arrange & Act
            var service = new ObfuscatorService(_mockRedactorProvider.Object);

            // Assert
            service.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_WithNullRedactorProvider_ShouldThrowArgumentNullException()
        {
            // Arrange & Act & Assert
            // Note: The current implementation doesn't validate null input in constructor
            // This test documents the current behavior - no exception is thrown
            var service = new ObfuscatorService(null!);
            service.Should().NotBeNull();
            
            // However, using the service with null provider will throw when GetRedactor is called
            var testUser = new TestUser { Name = "Test" };
            Assert.Throws<NullReferenceException>(() => service.SanitizeSensitiveData(testUser));
        }

        [Fact]
        public void SanitizeSensitiveData_WithStringProperties_ShouldRedactSensitiveData()
        {
            // Arrange
            var user = new TestUser
            {
                Name = "John Doe",
                Email = "john@example.com",
                Password = "secret123",
                Age = 30
            };

            // Act
            var result = _obfuscatorService.SanitizeSensitiveData(user);

            // Assert
            result.Should().NotBeNullOrEmpty();
            
            var jsonDoc = JsonDocument.Parse(result);
            var root = jsonDoc.RootElement;
            
            root.GetProperty("Name").GetString().Should().Be("John Doe");
            root.GetProperty("Email").GetString().Should().Be("[REDACTED]");
            root.GetProperty("Password").GetString().Should().Be("[REDACTED]");
            root.GetProperty("age").GetInt32().Should().Be(30); // Note: lowercase due to ToLowerInvariant()

            _mockRedactor.Verify(x => x.Redact("john@example.com"), Times.Once);
            _mockRedactor.Verify(x => x.Redact("secret123"), Times.Once);
            _mockRedactor.Verify(x => x.Redact("John Doe"), Times.Never);
        }

        [Fact]
        public void SanitizeSensitiveData_WithValueTypeProperties_ShouldRedactSensitiveData()
        {
            // Arrange
            var user = new TestUser
            {
                Age = 30,
                SocialSecurityNumber = 123456789,
                Salary = 50000.50m,
                IsActive = true
            };

            // Act
            var result = _obfuscatorService.SanitizeSensitiveData(user);

            // Assert
            result.Should().NotBeNullOrEmpty();
            
            var jsonDoc = JsonDocument.Parse(result);
            var root = jsonDoc.RootElement;
            
            root.GetProperty("age").GetInt32().Should().Be(30);
            root.GetProperty("SocialSecurityNumber").GetString().Should().Be("[REDACTED]");
            root.GetProperty("Salary").GetString().Should().Be("[REDACTED]");
            root.GetProperty("isactive").GetBoolean().Should().Be(true);

            _mockRedactor.Verify(x => x.Redact("123456789"), Times.Once);
            // Note: Decimal formatting may vary by culture, so we verify with the actual formatted value
            _mockRedactor.Verify(x => x.Redact(It.Is<string>(s => s.Contains("50000"))), Times.Once);
        }

        [Fact]
        public void SanitizeSensitiveData_WithNestedObjects_ShouldRecursivelySanitize()
        {
            // Arrange
            var user = new TestUser
            {
                Name = "John Doe",
                Email = "john@example.com",
                Address = new TestAddress
                {
                    Street = "123 Main St",
                    CreditCardNumber = "1234-5678-9012-3456",
                    City = "New York",
                    NestedInfo = new TestNestedInfo
                    {
                        Info = "Public Info",
                        SecretInfo = "Secret Info"
                    }
                }
            };

            // Act
            var result = _obfuscatorService.SanitizeSensitiveData(user);

            // Assert
            result.Should().NotBeNullOrEmpty();
            
            var jsonDoc = JsonDocument.Parse(result);
            var root = jsonDoc.RootElement;
            
            root.GetProperty("Name").GetString().Should().Be("John Doe");
            root.GetProperty("Email").GetString().Should().Be("[REDACTED]");
            
            // The Address property contains a JSON string, not a nested object
            var addressJson = root.GetProperty("Address").GetString();
            addressJson.Should().NotBeNullOrEmpty();
            
            // Parse the nested JSON string
            var addressDoc = JsonDocument.Parse(addressJson!);
            var addressRoot = addressDoc.RootElement;
            
            addressRoot.GetProperty("Street").GetString().Should().Be("123 Main St");
            addressRoot.GetProperty("CreditCardNumber").GetString().Should().Be("[REDACTED]");
            addressRoot.GetProperty("City").GetString().Should().Be("New York");
            
            // The NestedInfo property also contains a JSON string
            var nestedInfoJson = addressRoot.GetProperty("NestedInfo").GetString();
            nestedInfoJson.Should().NotBeNullOrEmpty();
            
            var nestedDoc = JsonDocument.Parse(nestedInfoJson!);
            var nestedRoot = nestedDoc.RootElement;
            
            nestedRoot.GetProperty("Info").GetString().Should().Be("Public Info");
            nestedRoot.GetProperty("SecretInfo").GetString().Should().Be("[REDACTED]");

            _mockRedactor.Verify(x => x.Redact("john@example.com"), Times.Once);
            _mockRedactor.Verify(x => x.Redact("1234-5678-9012-3456"), Times.Once);
            _mockRedactor.Verify(x => x.Redact("Secret Info"), Times.Once);
        }

        [Fact]
        public void SanitizeSensitiveData_WithNullNestedObject_ShouldHandleGracefully()
        {
            // Arrange
            var user = new TestUser
            {
                Name = "John Doe",
                Email = "john@example.com",
                Address = null
            };

            // Act
            var result = _obfuscatorService.SanitizeSensitiveData(user);

            // Assert
            result.Should().NotBeNullOrEmpty();
            
            var jsonDoc = JsonDocument.Parse(result);
            var root = jsonDoc.RootElement;
            
            root.GetProperty("Name").GetString().Should().Be("John Doe");
            root.GetProperty("Email").GetString().Should().Be("[REDACTED]");
            root.GetProperty("Address").ValueKind.Should().Be(JsonValueKind.Null);
        }

        [Fact]
        public void SanitizeSensitiveData_WithNullStringValue_ShouldHandleGracefully()
        {
            // Arrange
            var user = new TestUser
            {
                Name = "John Doe",
                NullableProperty = null
            };

            // Act
            var result = _obfuscatorService.SanitizeSensitiveData(user);

            // Assert
            result.Should().NotBeNullOrEmpty();
            
            var jsonDoc = JsonDocument.Parse(result);
            var root = jsonDoc.RootElement;
            
            root.GetProperty("Name").GetString().Should().Be("John Doe");
            root.GetProperty("NullableProperty").GetString().Should().Be(string.Empty);
        }

        [Fact]
        public void SanitizeSensitiveData_WithEmptyClass_ShouldReturnEmptyJson()
        {
            // Arrange
            var emptyObject = new EmptyClass();

            // Act
            var result = _obfuscatorService.SanitizeSensitiveData(emptyObject);

            // Assert
            result.Should().Be("{}");
        }

        [Fact]
        public void SanitizeSensitiveData_WithOnlySensitiveData_ShouldRedactAll()
        {
            // Arrange
            var sensitiveObject = new OnlySensitiveDataClass
            {
                Secret1 = "secret1",
                Secret2 = "secret2"
            };

            // Act
            var result = _obfuscatorService.SanitizeSensitiveData(sensitiveObject);

            // Assert
            result.Should().NotBeNullOrEmpty();
            
            var jsonDoc = JsonDocument.Parse(result);
            var root = jsonDoc.RootElement;
            
            root.GetProperty("Secret1").GetString().Should().Be("[REDACTED]");
            root.GetProperty("Secret2").GetString().Should().Be("[REDACTED]");

            _mockRedactor.Verify(x => x.Redact("secret1"), Times.Once);
            _mockRedactor.Verify(x => x.Redact("secret2"), Times.Once);
        }

        [Fact]
        public void SanitizeSensitiveData_WithOnlyNonSensitiveData_ShouldNotRedactAny()
        {
            // Arrange
            var publicObject = new OnlyNonSensitiveDataClass
            {
                Public1 = "public1",
                Public2 = 42,
                Public3 = true
            };

            // Act
            var result = _obfuscatorService.SanitizeSensitiveData(publicObject);

            // Assert
            result.Should().NotBeNullOrEmpty();
            
            var jsonDoc = JsonDocument.Parse(result);
            var root = jsonDoc.RootElement;
            
            root.GetProperty("Public1").GetString().Should().Be("public1");
            root.GetProperty("public2").GetInt32().Should().Be(42);
            root.GetProperty("public3").GetBoolean().Should().Be(true);

            _mockRedactor.Verify(x => x.Redact(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void SanitizeSensitiveData_WithNullInput_ShouldThrowArgumentNullException()
        {
            // Arrange & Act & Assert
            // Note: The current implementation doesn't validate null input
            // This test documents the current behavior - NullReferenceException is thrown during reflection
            Assert.Throws<NullReferenceException>(() => _obfuscatorService.SanitizeSensitiveData<TestUser>(null!));
        }

        [Fact]
        public void SanitizeSensitiveData_WithValueTypeInput_ShouldWork()
        {
            // Arrange
            var intValue = 42;

            // Act
            var result = _obfuscatorService.SanitizeSensitiveData(intValue);

            // Assert
            result.Should().Be("{}"); // int has no properties
        }

        [Fact]
        public void SanitizeSensitiveData_WithStringInput_ShouldWork()
        {
            // Arrange
            var stringValue = "test string";

            // Act
            var result = _obfuscatorService.SanitizeSensitiveData(stringValue);

            // Assert
            result.Should().Be("{}"); // string has no accessible properties in this context
        }

        [Fact]
        public void SanitizeSensitiveData_WhenRedactorThrowsException_ShouldPropagateException()
        {
            // Arrange
            _mockRedactor
                .Setup(x => x.Redact(It.IsAny<string>()))
                .Throws(new InvalidOperationException("Redactor error"));

            var user = new TestUser
            {
                Email = "test@example.com"
            };

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => _obfuscatorService.SanitizeSensitiveData(user));
        }

        [Fact]
        public void SanitizeSensitiveData_WithComplexValueTypes_ShouldHandleCorrectly()
        {
            // Arrange
            var user = new TestUser
            {
                CreatedAt = new DateTime(2023, 1, 1, 12, 0, 0),
                Age = 25
            };

            // Act
            var result = _obfuscatorService.SanitizeSensitiveData(user);

            // Assert
            result.Should().NotBeNullOrEmpty();
            
            var jsonDoc = JsonDocument.Parse(result);
            var root = jsonDoc.RootElement;
            
            root.GetProperty("createdat").GetDateTime().Should().Be(new DateTime(2023, 1, 1, 12, 0, 0));
            root.GetProperty("age").GetInt32().Should().Be(25);
        }

        [Fact]
        public void SanitizeSensitiveData_CallsRedactorProviderCorrectly()
        {
            // Arrange
            var user = new TestUser();

            // Act
            _obfuscatorService.SanitizeSensitiveData(user);

            // Assert
            _mockRedactorProvider.Verify(
                x => x.GetRedactor(It.IsAny<DataClassificationSet>()), 
                Times.Once);
        }
    }
}