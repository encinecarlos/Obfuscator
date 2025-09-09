using System;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Obfuscator.Extensions;
using Obfuscator.Interfaces;
using Obfuscator.Tests.TestModels;
using Xunit;

namespace Obfuscator.Tests.Integration
{
    public class ObfuscatorIntegrationTests
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IObfuscatorService _obfuscatorService;

        public ObfuscatorIntegrationTests()
        {
            var services = new ServiceCollection();
            services.AddObfuscator();
            
            _serviceProvider = services.BuildServiceProvider();
            _obfuscatorService = _serviceProvider.GetRequiredService<IObfuscatorService>();
        }

        [Fact]
        public void EndToEndTest_ComplexObjectWithNestedSensitiveData_ShouldRedactCorrectly()
        {
            // Arrange
            var testUser = new TestUser
            {
                Name = "John Doe",
                Email = "john.doe@example.com",
                Password = "MySecretPassword123!",
                Age = 35,
                SocialSecurityNumber = 123456789,
                Salary = 75000.50m,
                IsActive = true,
                CreatedAt = new DateTime(2023, 1, 15, 10, 30, 0),
                NullableProperty = "Some nullable value",
                Address = new TestAddress
                {
                    Street = "123 Main Street",
                    CreditCardNumber = "4532-1234-5678-9012",
                    City = "New York",
                    NestedInfo = new TestNestedInfo
                    {
                        Info = "Public information",
                        SecretInfo = "This is very secret information"
                    }
                }
            };

            // Act
            var result = _obfuscatorService.SanitizeSensitiveData(testUser);

            // Assert
            result.Should().NotBeNullOrEmpty();
            
            var jsonDoc = JsonDocument.Parse(result);
            var root = jsonDoc.RootElement;

            // Verify non-sensitive data is preserved
            root.GetProperty("Name").GetString().Should().Be("John Doe");
            root.GetProperty("age").GetInt32().Should().Be(35);
            root.GetProperty("isactive").GetBoolean().Should().Be(true);
            root.GetProperty("createdat").GetDateTime().Should().Be(new DateTime(2023, 1, 15, 10, 30, 0));
            root.GetProperty("NullableProperty").GetString().Should().Be("Some nullable value");

            // Verify sensitive data is redacted
            root.GetProperty("Email").GetString().Should().Be("[REDACTED]");
            root.GetProperty("Password").GetString().Should().Be("[REDACTED]");
            root.GetProperty("SocialSecurityNumber").GetString().Should().Be("[REDACTED]");
            root.GetProperty("Salary").GetString().Should().Be("[REDACTED]");

            // Verify nested object handling
            var addressJson = root.GetProperty("Address").GetString();
            addressJson.Should().NotBeNullOrEmpty();
            
            var addressDoc = JsonDocument.Parse(addressJson!);
            var addressRoot = addressDoc.RootElement;
            
            addressRoot.GetProperty("Street").GetString().Should().Be("123 Main Street");
            addressRoot.GetProperty("City").GetString().Should().Be("New York");
            addressRoot.GetProperty("CreditCardNumber").GetString().Should().Be("[REDACTED]");
            
            // Verify deeply nested object handling
            var nestedInfoJson = addressRoot.GetProperty("NestedInfo").GetString();
            nestedInfoJson.Should().NotBeNullOrEmpty();
            
            var nestedDoc = JsonDocument.Parse(nestedInfoJson!);
            var nestedRoot = nestedDoc.RootElement;
            
            nestedRoot.GetProperty("Info").GetString().Should().Be("Public information");
            nestedRoot.GetProperty("SecretInfo").GetString().Should().Be("[REDACTED]");
        }

        [Fact]
        public void EndToEndTest_RealWorldScenario_UserRegistration()
        {
            // Arrange - Simulate a user registration scenario
            var registrationData = new
            {
                Username = "johndoe123",
                Email = "john.doe@email.com",
                Password = "SuperSecret123!",
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = new DateTime(1988, 5, 15),
                PhoneNumber = "+1-555-123-4567",
                IsEmailVerified = false
            };

            // Act
            var sanitizedResult = _obfuscatorService.SanitizeSensitiveData(registrationData);

            // Assert
            sanitizedResult.Should().NotBeNullOrEmpty();
            
            var jsonDoc = JsonDocument.Parse(sanitizedResult);
            var root = jsonDoc.RootElement;

            // All properties should be preserved as-is since they don't have [SensitiveData] attribute
            root.GetProperty("Username").GetString().Should().Be("johndoe123");
            root.GetProperty("Email").GetString().Should().Be("john.doe@email.com");
            root.GetProperty("Password").GetString().Should().Be("SuperSecret123!");
            root.GetProperty("FirstName").GetString().Should().Be("John");
            root.GetProperty("LastName").GetString().Should().Be("Doe");
            root.GetProperty("PhoneNumber").GetString().Should().Be("+1-555-123-4567");
            root.GetProperty("isemailverified").GetBoolean().Should().Be(false);
        }

        [Fact]
        public void EndToEndTest_PerformanceWithLargeObject()
        {
            // Arrange - Create a user with large nested structure
            var largeUser = new TestUser
            {
                Name = "Performance Test User",
                Email = "performance@test.com",
                Password = "password123",
                Address = new TestAddress
                {
                    Street = "Performance Street",
                    CreditCardNumber = "1234-5678-9012-3456",
                    City = "Performance City",
                    NestedInfo = new TestNestedInfo
                    {
                        Info = new string('a', 10000), // Large string
                        SecretInfo = new string('s', 10000) // Large sensitive string
                    }
                }
            };

            // Act
            var startTime = DateTime.UtcNow;
            var result = _obfuscatorService.SanitizeSensitiveData(largeUser);
            var endTime = DateTime.UtcNow;
            var duration = endTime - startTime;

            // Assert
            result.Should().NotBeNullOrEmpty();
            duration.Should().BeLessThan(TimeSpan.FromSeconds(5)); // Performance threshold
            
            var jsonDoc = JsonDocument.Parse(result);
            var root = jsonDoc.RootElement;
            
            root.GetProperty("Email").GetString().Should().Be("[REDACTED]");
            root.GetProperty("Password").GetString().Should().Be("[REDACTED]");
        }

        [Fact]
        public void EndToEndTest_ErrorHandling_InvalidNestedObject()
        {
            // Arrange - Create an object that might cause issues
            var userWithCircularRef = new TestUser
            {
                Name = "Test User",
                Email = "test@example.com"
            };

            // Act & Assert - Should not throw
            var result = _obfuscatorService.SanitizeSensitiveData(userWithCircularRef);
            result.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void EndToEndTest_ServiceLifetime_MultipleCalls()
        {
            // Arrange
            var user1 = new TestUser { Email = "user1@test.com", Name = "User 1" };
            var user2 = new TestUser { Email = "user2@test.com", Name = "User 2" };

            // Act
            var result1 = _obfuscatorService.SanitizeSensitiveData(user1);
            var result2 = _obfuscatorService.SanitizeSensitiveData(user2);

            // Assert
            result1.Should().NotBeNullOrEmpty();
            result2.Should().NotBeNullOrEmpty();
            result1.Should().NotBe(result2);

            var json1 = JsonDocument.Parse(result1);
            var json2 = JsonDocument.Parse(result2);

            json1.RootElement.GetProperty("Name").GetString().Should().Be("User 1");
            json2.RootElement.GetProperty("Name").GetString().Should().Be("User 2");
            
            json1.RootElement.GetProperty("Email").GetString().Should().Be("[REDACTED]");
            json2.RootElement.GetProperty("Email").GetString().Should().Be("[REDACTED]");
        }

        private void Dispose()
        {
            (_serviceProvider as IDisposable)?.Dispose();
        }
    }
}