using Microsoft.Extensions.Compliance.Classification;
using Microsoft.Extensions.Compliance.Redaction;
using Obfuscator.Interfaces;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Obfuscator
{
    public class ObfuscatorService : IObfuscatorService
    {
        private readonly IRedactorProvider _redactorProvider;

        public ObfuscatorService(IRedactorProvider redactorProvider)
        {
            _redactorProvider = redactorProvider;
        }

        public string SanitizeSensitiveData<T>(T input)
        {
            var readctor = _redactorProvider.GetRedactor(new DataClassificationSet());

            var type = typeof(T);

            var valueObjects = new Dictionary<string, object>();

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in properties)
            {
                var attr = prop.GetCustomAttribute(typeof(SensitiveDataAttribute));
                var propValue = prop.GetValue(input);

                var propertySerialized = prop.GetCustomAttribute<JsonPropertyNameAttribute>();

                var propertyName = propertySerialized.Name ?? prop.Name;

                if (prop.PropertyType == typeof(string))
                {
                    var strValue = propValue as string ?? String.Empty;
                    valueObjects[prop.Name] = attr != null ? readctor.Redact(strValue) : strValue;
                }
                else if (prop.PropertyType.IsValueType)
                {
                    if (attr != null && propValue != null)
                        valueObjects[prop.Name] = readctor.Redact(propValue.ToString());
                    else
                        valueObjects[propertyName] = propValue;

                }
                else if (prop.PropertyType.IsClass)
                {
                    valueObjects[propertyName] = propValue != null ? SanitizeSensitiveData(propValue) : null;
                }
                else
                {
                    valueObjects[propertyName] = prop.GetValue(input) ?? "null";
                }
            }

            return JsonSerializer.Serialize(valueObjects);
        }
    }
}