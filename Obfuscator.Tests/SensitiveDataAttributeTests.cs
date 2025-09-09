using System;
using FluentAssertions;
using Obfuscator.Tests.TestModels;
using Xunit;

namespace Obfuscator.Tests
{
    public class SensitiveDataAttributeTests
    {
        [Fact]
        public void SensitiveDataAttribute_CanBeAppliedToProperty()
        {
            // Arrange
            var propertyInfo = typeof(TestUser).GetProperty(nameof(TestUser.Email));

            // Act
            var attribute = Attribute.GetCustomAttribute(propertyInfo!, typeof(SensitiveDataAttribute));

            // Assert
            attribute.Should().NotBeNull();
            attribute.Should().BeOfType<SensitiveDataAttribute>();
        }

        [Fact]
        public void SensitiveDataAttribute_AllowsMultipleFalse()
        {
            // Arrange & Act
            var attributeUsage = (AttributeUsageAttribute)Attribute.GetCustomAttribute(
                typeof(SensitiveDataAttribute), 
                typeof(AttributeUsageAttribute))!;

            // Assert
            attributeUsage.Should().NotBeNull();
            attributeUsage.AllowMultiple.Should().BeFalse();
        }

        [Fact]
        public void SensitiveDataAttribute_TargetsProperty()
        {
            // Arrange & Act
            var attributeUsage = (AttributeUsageAttribute)Attribute.GetCustomAttribute(
                typeof(SensitiveDataAttribute), 
                typeof(AttributeUsageAttribute))!;

            // Assert
            attributeUsage.Should().NotBeNull();
            attributeUsage.ValidOn.Should().Be(AttributeTargets.Property);
        }

        [Fact]
        public void SensitiveDataAttribute_CanBeInstantiated()
        {
            // Arrange & Act
            var attribute = new SensitiveDataAttribute();

            // Assert
            attribute.Should().NotBeNull();
            attribute.Should().BeOfType<SensitiveDataAttribute>();
        }

        [Fact]
        public void SensitiveDataAttribute_InheritsFromAttribute()
        {
            // Arrange & Act
            var attribute = new SensitiveDataAttribute();

            // Assert
            attribute.Should().BeAssignableTo<Attribute>();
        }

        [Fact]
        public void SensitiveDataAttribute_HasCorrectTypeId()
        {
            // Arrange
            var attribute = new SensitiveDataAttribute();

            // Act
            var typeId = attribute.TypeId;

            // Assert
            typeId.Should().Be(typeof(SensitiveDataAttribute));
        }
    }
}