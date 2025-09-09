using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using Obfuscator.Redactors;

namespace Obfuscator.Tests.Debug
{
    class TestUser
    {
        public string Name { get; set; } = string.Empty;
        
        [SensitiveData]
        public string Email { get; set; } = string.Empty;
        
        public SimpleAddress? Address { get; set; }
        
        public int Age { get; set; }
    }
    
    class SimpleAddress
    {
        public string Street { get; set; } = string.Empty;
        
        [SensitiveData]
        public string CreditCardNumber { get; set; } = string.Empty;
        
        public string City { get; set; } = string.Empty;
    }
    
    class Program
    {
        static void Main()
        {
            var provider = new SimpleRedactorProvider();
            var service = new ObfuscatorService(provider);
            
            var user = new TestUser
            {
                Name = "John Doe",
                Email = "john@example.com",
                Age = 30,
                Address = new SimpleAddress
                {
                    Street = "123 Main St",
                    CreditCardNumber = "1234-5678-9012-3456",
                    City = "New York"
                }
            };

            Console.WriteLine("=== Manual simulation of ObfuscatorService logic ===");
            
            var type = typeof(TestUser);
            var valueObjects = new Dictionary<string, object>();
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            
            foreach (var prop in properties)
            {
                Console.WriteLine($"\nProcessing property: {prop.Name}");
                Console.WriteLine($"Property type: {prop.PropertyType.Name}");
                
                var attr = prop.GetCustomAttribute(typeof(SensitiveDataAttribute));
                var propValue = prop.GetValue(user);
                
                Console.WriteLine($"Has SensitiveData attribute: {attr != null}");
                Console.WriteLine($"Property value: {propValue}");
                Console.WriteLine($"Is string: {prop.PropertyType == typeof(string)}");
                Console.WriteLine($"Is value type: {prop.PropertyType.IsValueType}");
                Console.WriteLine($"Is class: {prop.PropertyType.IsClass}");

                if (prop.PropertyType == typeof(string))
                {
                    var strValue = propValue as string ?? String.Empty;
                    valueObjects[prop.Name] = attr != null ? "[REDACTED]" : strValue;
                    Console.WriteLine($"Result: {valueObjects[prop.Name]}");
                }
                else if (prop.PropertyType.IsValueType)
                {
                    if (attr != null && propValue != null)
                        valueObjects[prop.Name] = "[REDACTED]";
                    else
                        valueObjects[prop.Name.ToLowerInvariant()] = propValue;
                    Console.WriteLine($"Result: {valueObjects[prop.Name.ToLowerInvariant()]}");
                }
                else if (prop.PropertyType.IsClass)
                {
                    if (propValue != null)
                    {
                        Console.WriteLine("Calling recursive SanitizeSensitiveData...");
                        var recursiveResult = service.SanitizeSensitiveData(propValue);
                        Console.WriteLine($"Recursive result: {recursiveResult}");
                        valueObjects[prop.Name] = recursiveResult;
                    }
                    else
                    {
                        valueObjects[prop.Name] = null;
                    }
                    Console.WriteLine($"Final result: {valueObjects[prop.Name]}");
                }
                else
                {
                    valueObjects[prop.Name] = prop.GetValue(user) ?? "null";
                    Console.WriteLine($"Result: {valueObjects[prop.Name]}");
                }
            }

            var finalJson = JsonSerializer.Serialize(valueObjects);
            Console.WriteLine($"\n=== Final JSON ===");
            Console.WriteLine(finalJson);
            
            Console.WriteLine($"\n=== Actual service result ===");
            var actualResult = service.SanitizeSensitiveData(user);
            Console.WriteLine(actualResult);
        }
    }
}