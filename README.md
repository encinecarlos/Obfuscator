# EncineCarlos.Obfuscator

[![NuGet Version](https://img.shields.io/nuget/v/EncineCarlos.Obfuscator.svg)](https://www.nuget.org/packages/EncineCarlos.Obfuscator/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/EncineCarlos.Obfuscator.svg)](https://www.nuget.org/packages/EncineCarlos.Obfuscator/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A powerful and easy-to-use .NET library for obfuscating sensitive data in your applications. Automatically redacts properties marked with `[SensitiveData]` attribute using the Microsoft Extensions Compliance framework.

## Features

✅ **Attribute-based Obfuscation**: Simply mark properties with `[SensitiveData]` attribute  
✅ **Nested Object Support**: Recursively processes complex object hierarchies  
✅ **JSON Serialization**: Returns clean JSON with sensitive data redacted  
✅ **JsonPropertyName Support**: Respects `[JsonPropertyName]` attributes  
✅ **Microsoft Extensions Integration**: Built on Microsoft.Extensions.Compliance framework  
✅ **Dependency Injection Ready**: Easy integration with .NET DI container  
✅ **High Performance**: Minimal overhead with reflection caching  

## Installation

```bash
dotnet add package EncineCarlos.Obfuscator
```

## Quick Start

### 1. Mark sensitive properties with `[SensitiveData]`

```csharp
public class User
{
    public string Name { get; set; }
    
    [SensitiveData]
    public string Email { get; set; }
    
    [SensitiveData]
    public string Password { get; set; }
    
    public int Age { get; set; }
    
    [SensitiveData]
    public decimal Salary { get; set; }
}
```

### 2. Register the service in your DI container

```csharp
// Program.cs or Startup.cs
services.AddObfuscator();
```

### 3. Use the obfuscator service

```csharp
public class MyController : ControllerBase
{
    private readonly IObfuscatorService _obfuscator;
    
    public MyController(IObfuscatorService obfuscator)
    {
        _obfuscator = obfuscator;
    }
    
    public IActionResult GetUser()
    {
        var user = new User
        {
            Name = "John Doe",
            Email = "john@example.com",
            Password = "secret123",
            Age = 30,
            Salary = 75000.50m
        };
        
        // Obfuscate sensitive data before logging or returning
        var obfuscatedData = _obfuscator.ObfuscateSensitiveData(user);
        
        _logger.LogInformation("User data: {UserData}", obfuscatedData);
        
        return Ok(obfuscatedData);
    }
}
```

### Output

```json
{
  "Name": "John Doe",
  "Email": "[REDACTED]",
  "Password": "[REDACTED]",
  "Age": 30,
  "Salary": "[REDACTED]"
}
```

## Advanced Usage

### Nested Objects

The library automatically handles nested objects:

```csharp
public class User
{
    public string Name { get; set; }
    
    [SensitiveData]
    public string Email { get; set; }
    
    public Address Address { get; set; }
}

public class Address
{
    public string Street { get; set; }
    
    [SensitiveData]
    public string CreditCardNumber { get; set; }
    
    public string City { get; set; }
}
```

### JsonPropertyName Support

Respects `[JsonPropertyName]` attributes for consistent JSON output:

```csharp
public class User
{
    [JsonPropertyName("full_name")]
    public string Name { get; set; }
    
    [SensitiveData]
    [JsonPropertyName("email_address")]
    public string Email { get; set; }
}
```

### Manual Usage (without DI)

```csharp
var redactorProvider = new SimpleRedactorProvider();
var obfuscator = new ObfuscatorService(redactorProvider);

var result = obfuscator.ObfuscateSensitiveData(myObject);
```

## Use Cases

- **Logging**: Remove sensitive data from logs
- **API Responses**: Clean data before sending to clients
- **Debugging**: Safe object inspection in development
- **Compliance**: GDPR, HIPAA, and other privacy regulations
- **Data Export**: Sanitize data for reports or analytics

## Configuration

The library uses Microsoft.Extensions.Compliance for redaction. You can customize the redaction behavior:

```csharp
services.AddRedaction(builder =>
{
    builder.SetRedactor<MyCustomRedactor>(new DataClassificationSet());
});
```

## Performance

The library is optimized for performance:
- Uses reflection caching internally
- Minimal memory allocations
- Supports high-throughput scenarios

## Requirements

- **.NET Standard 2.1** or higher
- **Microsoft.Extensions.Compliance.Abstractions** 9.8.0+
- **Microsoft.Extensions.Compliance.Redaction** 9.8.0+

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

If you encounter any issues or have questions, please [open an issue](https://github.com/encinecarlos/obfuscator/issues) on GitHub.

---

Made with ❤️ by [Carlos Encine](https://github.com/encinecarlos)
