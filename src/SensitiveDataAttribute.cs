using System;

namespace Obfuscator
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class SensitiveDataAttribute : Attribute
    {
        
    }
}