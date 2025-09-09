using System;
using FluentAssertions;
using Microsoft.Extensions.Compliance.Classification;
using Microsoft.Extensions.Compliance.Redaction;
using Obfuscator.Redactors;
using Xunit;

namespace Obfuscator.Tests.Redactors
{
    public class SimpleRedactorProviderTests
    {
        private readonly SimpleRedactorProvider _provider;

        public SimpleRedactorProviderTests()
        {
            _provider = new SimpleRedactorProvider();
        }

        [Fact]
        public void GetRedactor_WithAnyDataClassificationSet_ShouldReturnRedactor()
        {
            // Arrange
            var classifications = new DataClassificationSet();

            // Act
            var redactor = _provider.GetRedactor(classifications);

            // Assert
            redactor.Should().NotBeNull();
            redactor.Should().BeAssignableTo<Redactor>();
        }

        [Fact]
        public void GetRedactor_WithNullDataClassificationSet_ShouldReturnRedactor()
        {
            // Arrange & Act
            var redactor = _provider.GetRedactor(null!);

            // Assert
            redactor.Should().NotBeNull();
            redactor.Should().BeAssignableTo<Redactor>();
        }

        [Fact]
        public void GetRedactor_MultipleCalls_ShouldReturnNewInstances()
        {
            // Arrange
            var classifications = new DataClassificationSet();

            // Act
            var redactor1 = _provider.GetRedactor(classifications);
            var redactor2 = _provider.GetRedactor(classifications);

            // Assert
            redactor1.Should().NotBeSameAs(redactor2);
        }

        [Fact]
        public void SimpleRedactor_RedactString_WithNullInput_ShouldReturnEmptyString()
        {
            // Arrange
            var redactor = _provider.GetRedactor(new DataClassificationSet());

            // Act
            var result = redactor.Redact((string?)null);

            // Assert
            result.Should().Be(string.Empty);
        }

        [Fact]
        public void SimpleRedactor_RedactString_WithValidInput_ShouldReturnRedacted()
        {
            // Arrange
            var redactor = _provider.GetRedactor(new DataClassificationSet());
            var input = "sensitive data";

            // Act
            var result = redactor.Redact(input);

            // Assert
            result.Should().Be("[REDACTED]");
        }

        [Fact]
        public void SimpleRedactor_RedactString_WithEmptyInput_ShouldReturnRedacted()
        {
            // Arrange
            var redactor = _provider.GetRedactor(new DataClassificationSet());
            var input = string.Empty;

            // Act
            var result = redactor.Redact(input);

            // Assert
            result.Should().Be("[REDACTED]");
        }

        [Fact]
        public void SimpleRedactor_RedactString_WithWhitespaceInput_ShouldReturnRedacted()
        {
            // Arrange
            var redactor = _provider.GetRedactor(new DataClassificationSet());
            var input = "   ";

            // Act
            var result = redactor.Redact(input);

            // Assert
            result.Should().Be("[REDACTED]");
        }

        [Fact]
        public void SimpleRedactor_GetRedactedLength_ShouldReturnInputLength()
        {
            // Arrange
            var redactor = _provider.GetRedactor(new DataClassificationSet());
            var input = "test input".AsSpan();

            // Act
            var result = redactor.GetRedactedLength(input);

            // Assert
            result.Should().Be(input.Length);
        }

        [Fact]
        public void SimpleRedactor_GetRedactedLength_WithEmptyInput_ShouldReturnZero()
        {
            // Arrange
            var redactor = _provider.GetRedactor(new DataClassificationSet());
            var input = ReadOnlySpan<char>.Empty;

            // Act
            var result = redactor.GetRedactedLength(input);

            // Assert
            result.Should().Be(0);
        }

        [Fact]
        public void SimpleRedactor_RedactSpan_ShouldThrowNotImplementedException()
        {
            // Arrange
            var redactor = _provider.GetRedactor(new DataClassificationSet());
            var sourceArray = "test".ToCharArray();
            var destinationArray = new char[10];

            // Act & Assert
            Assert.Throws<NotImplementedException>(() => 
                redactor.Redact(sourceArray.AsSpan(), destinationArray.AsSpan()));
        }

        [Theory]
        [InlineData("")]
        [InlineData("a")]
        [InlineData("test")]
        [InlineData("very long sensitive information")]
        [InlineData("123456789")]
        [InlineData("special!@#$%^&*()characters")]
        public void SimpleRedactor_RedactString_WithVariousInputs_ShouldAlwaysReturnRedacted(string input)
        {
            // Arrange
            var redactor = _provider.GetRedactor(new DataClassificationSet());

            // Act
            var result = redactor.Redact(input);

            // Assert
            result.Should().Be("[REDACTED]");
        }
    }
}