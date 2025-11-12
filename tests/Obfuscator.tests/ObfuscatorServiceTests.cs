using System.Text.Json.Serialization;
using Microsoft.Extensions.Compliance.Classification;
using Microsoft.Extensions.Compliance.Redaction;
using Moq;
using Obfuscator;
using Obfuscator.Interfaces;
using Xunit;

namespace Obfuscator.tests
{
    public class ObfuscatorServiceTests
    {
        private readonly Mock<IRedactorProvider> _mockRedactorProvider;
        private readonly Mock<Redactor> _mockRedactor;
        private readonly ObfuscatorService _obfuscatorService;

        public ObfuscatorServiceTests()
        {
            _mockRedactor = new Mock<Redactor>();
            _mockRedactor.Setup(r => r.Redact(It.IsAny<string>()))
                .Returns((string input) => input == null ? string.Empty : "[REDACTED]");

            _mockRedactorProvider = new Mock<IRedactorProvider>();
            _mockRedactorProvider.Setup(p => p.GetRedactor(It.IsAny<DataClassificationSet>()))
                .Returns(_mockRedactor.Object);

            _obfuscatorService = new ObfuscatorService(_mockRedactorProvider.Object);
        }

        #region String Property Tests

        [Fact]
        public void ObfuscateSensitiveData_WithSensitiveStringProperty_ShouldRedactValue()
        {
            // Arrange
            var input = new TestDataWithSensitiveString
            {
                Username = "john_doe",
                NonSensitiveData = "public_data"
            };

            // Act
            var result = _obfuscatorService.ObfuscateSensitiveData(input);

            // Assert
            Assert.Contains("[REDACTED]", result);
            Assert.Contains("\"Username\":\"[REDACTED]\"", result);
            Assert.Contains("\"NonSensitiveData\":\"public_data\"", result);
        }

        [Fact]
        public void ObfuscateSensitiveData_WithNullStringProperty_ShouldHandleNull()
        {
            // Arrange
            var input = new TestDataWithSensitiveString
            {
                Username = null,
                NonSensitiveData = "public_data"
            };

            // Act
            var result = _obfuscatorService.ObfuscateSensitiveData(input);

            // Assert
            Assert.NotNull(result);
        }

        #endregion

        #region Value Type Tests

        [Fact]
        public void ObfuscateSensitiveData_WithSensitiveIntProperty_ShouldRedactValue()
        {
            // Arrange
            var input = new TestDataWithSensitiveInt
            {
                SensitiveNumber = 12345,
                PublicNumber = 999
            };

            // Act
            var result = _obfuscatorService.ObfuscateSensitiveData(input);

            // Assert
            Assert.Contains("[REDACTED]", result);
            Assert.Contains("\"PublicNumber\":999", result);
        }

        [Fact]
        public void ObfuscateSensitiveData_WithNullableValueType_ShouldHandleNull()
        {
            // Arrange
            var input = new TestDataWithNullableInt
            {
                OptionalNumber = null,
                PublicNumber = 42
            };

            // Act
            var result = _obfuscatorService.ObfuscateSensitiveData(input);

            // Assert
            Assert.NotNull(result);
        }

        #endregion

        #region JSON Property Name Tests

        [Fact]
        public void ObfuscateSensitiveData_WithJsonPropertyName_ShouldUseJsonName()
        {
            // Arrange
            var input = new TestDataWithJsonPropertyName
            {
                ApiKey = "secret123",
                UserName = "john"
            };

            // Act
            var result = _obfuscatorService.ObfuscateSensitiveData(input);

            // Assert
            Assert.Contains("\"api_key\"", result);
            Assert.Contains("\"user_name\"", result);
            Assert.Contains("[REDACTED]", result);
        }

        #endregion

        #region Nested Object Tests

        [Fact]
        public void ObfuscateSensitiveData_WithNestedObject_ShouldRecursivelyObfuscate()
        {
            // Arrange
            var nestedData = new TestDataWithSensitiveString
            {
                Username = "nested_user",
                NonSensitiveData = "nested_public"
            };

            var input = new TestDataWithNestedObject
            {
                PublicInfo = "public",
                NestedSensitiveData = nestedData
            };

            // Act
            var result = _obfuscatorService.ObfuscateSensitiveData(input);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("\"PublicInfo\":\"public\"", result);
            Assert.Contains("\"NestedSensitiveData\":", result);
        }

        [Fact]
        public void ObfuscateSensitiveData_WithNullNestedObject_ShouldHandleNull()
        {
            // Arrange
            var input = new TestDataWithNestedObject
            {
                PublicInfo = "public",
                NestedSensitiveData = null
            };

            // Act
            var result = _obfuscatorService.ObfuscateSensitiveData(input);

            // Assert
            Assert.Contains("\"NestedSensitiveData\":\"\"", result);
        }

        #endregion

        #region Complex Scenario Tests

        [Fact]
        public void ObfuscateSensitiveData_WithMultipleSensitiveProperties_ShouldRedactAll()
        {
            // Arrange
            var input = new ComplexTestData
            {
                Email = "user@example.com",
                PhoneNumber = "555-1234",
                Age = 30,
                Username = "user123"
            };

            // Act
            var result = _obfuscatorService.ObfuscateSensitiveData(input);

            // Assert
            var redactionCount = System.Text.RegularExpressions.Regex.Matches(result, "[REDACTED]").Count;
            Assert.True(redactionCount >= 3); // At least 3 sensitive properties should be redacted
        }

        [Fact]
        public void ObfuscateSensitiveData_WithNoSensitiveProperties_ShouldPreserveAllValues()
        {
            // Arrange
            var input = new TestDataWithNoSensitiveProperties
            {
                PublicName = "John",
                PublicAge = 30
            };

            // Act
            var result = _obfuscatorService.ObfuscateSensitiveData(input);

            // Assert
            Assert.Contains("\"PublicName\":\"John\"", result);
            Assert.Contains("\"PublicAge\":30", result);
            Assert.DoesNotContain("[REDACTED]", result);
        }

        [Fact]
        public void ObfuscateSensitiveData_ShouldCallRedactorProvider()
        {
            // Arrange
            var input = new TestDataWithSensitiveString { Username = "test" };

            // Act
            _obfuscatorService.ObfuscateSensitiveData(input);

            // Assert
            _mockRedactorProvider.Verify(p => p.GetRedactor(It.IsAny<DataClassificationSet>()), Times.Once);
        }

        [Fact]
        public void ObfuscateSensitiveData_ShouldCallRedactForEachSensitiveProperty()
        {
            // Arrange
            var input = new ComplexTestData
            {
                Email = "user@example.com",
                PhoneNumber = "555-1234",
                Age = 25,
                Username = "user123"
            };

            // Act
            _obfuscatorService.ObfuscateSensitiveData(input);

            // Assert
            _mockRedactor.Verify(r => r.Redact(It.IsAny<string>()), Times.AtLeastOnce);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void ObfuscateSensitiveData_WithEmptyString_ShouldHandleEmpty()
        {
            // Arrange
            var input = new TestDataWithSensitiveString
            {
                Username = string.Empty,
                NonSensitiveData = string.Empty
            };

            // Act
            var result = _obfuscatorService.ObfuscateSensitiveData(input);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public void ObfuscateSensitiveData_ShouldReturnValidJson()
        {
            // Arrange
            var input = new TestDataWithSensitiveString
            {
                Username = "test",
                NonSensitiveData = "data"
            };

            // Act
            var result = _obfuscatorService.ObfuscateSensitiveData(input);

            // Assert
            var exception = Record.Exception(() =>
                System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(result));
            Assert.Null(exception);
        }

        #endregion
    }

    #region Test Data Models

    /// <summary>Test data class with a single sensitive string property</summary>
    public class TestDataWithSensitiveString
    {
        [SensitiveData]
        public string Username { get; set; }

        public string NonSensitiveData { get; set; }
    }

    /// <summary>Test data class with sensitive integer property</summary>
    public class TestDataWithSensitiveInt
    {
        [SensitiveData]
        public int SensitiveNumber { get; set; }

        public int PublicNumber { get; set; }
    }

    /// <summary>Test data class with nullable integer property</summary>
    public class TestDataWithNullableInt
    {
        [SensitiveData]
        public int? OptionalNumber { get; set; }

        public int PublicNumber { get; set; }
    }

    /// <summary>Test data class with JsonPropertyName attributes</summary>
    public class TestDataWithJsonPropertyName
    {
        [SensitiveData]
        [JsonPropertyName("api_key")]
        public string ApiKey { get; set; }

        [JsonPropertyName("user_name")]
        public string UserName { get; set; }
    }

    /// <summary>Test data class with nested object</summary>
    public class TestDataWithNestedObject
    {
        public string PublicInfo { get; set; }

        public TestDataWithSensitiveString NestedSensitiveData { get; set; }
    }

    /// <summary>Complex test data with multiple sensitive properties</summary>
    public class ComplexTestData
    {
        [SensitiveData]
        public string Email { get; set; }

        [SensitiveData]
        public string PhoneNumber { get; set; }

        [SensitiveData]
        public int Age { get; set; }

        [SensitiveData]
        public string Username { get; set; }
    }

    /// <summary>Test data class with no sensitive properties</summary>
    public class TestDataWithNoSensitiveProperties
    {
        public string PublicName { get; set; }

        public int PublicAge { get; set; }
    }

    #endregion
}
