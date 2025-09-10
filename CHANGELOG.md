# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2025-09-10

### Added
- **Initial release** of EncineCarlos.Obfuscator library
- Attribute-based obfuscation with `[SensitiveData]` marking system
- Support for `JsonPropertyName` attribute in property serialization
- Enhanced nested object handling for complex data structures
- Comprehensive redaction capabilities using Microsoft Extensions Compliance framework
- Complete integration with dependency injection container through `AddObfuscator()` extension
- High-performance reflection-based property processing
- Support for string, value types, and complex object obfuscation
- Recursive processing of nested objects and collections
- JSON serialization with clean, redacted output
- Robust null value handling across all property types
- Extensive unit test coverage ensuring reliability
- Documentation and examples for common use cases

### Features
- **ObfuscatorService**: Core service for data obfuscation
- **SensitiveDataAttribute**: Simple attribute to mark sensitive properties
- **IObfuscatorService**: Clean interface for dependency injection
- **ObfuscatorExtensions**: Easy registration with DI container
- **SimpleRedactorProvider**: Built-in redaction provider
- Support for .NET Standard 2.1 and higher
- Compatible with Microsoft Extensions Compliance ecosystem
- Source Link support for enhanced debugging experience

### Use Cases
- Secure logging without sensitive data exposure
- API response sanitization
- Development and debugging data safety
- Compliance with GDPR, HIPAA, and other privacy regulations
- Data export and analytics preparation
