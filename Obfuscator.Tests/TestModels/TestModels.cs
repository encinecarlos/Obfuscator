using System;

namespace Obfuscator.Tests.TestModels
{
    public class TestUser
    {
        public string Name { get; set; } = string.Empty;
        
        [SensitiveData]
        public string Email { get; set; } = string.Empty;
        
        [SensitiveData]
        public string Password { get; set; } = string.Empty;
        
        public int Age { get; set; }
        
        [SensitiveData]
        public int SocialSecurityNumber { get; set; }
        
        public TestAddress? Address { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        [SensitiveData]
        public decimal Salary { get; set; }
        
        public bool IsActive { get; set; }
        
        public string? NullableProperty { get; set; }
    }
    
    public class TestAddress
    {
        public string Street { get; set; } = string.Empty;
        
        [SensitiveData]
        public string CreditCardNumber { get; set; } = string.Empty;
        
        public string City { get; set; } = string.Empty;
        
        public TestNestedInfo? NestedInfo { get; set; }
    }
    
    public class TestNestedInfo
    {
        public string Info { get; set; } = string.Empty;
        
        [SensitiveData]
        public string SecretInfo { get; set; } = string.Empty;
    }
    
    public class EmptyClass
    {
    }
    
    public class OnlySensitiveDataClass
    {
        [SensitiveData]
        public string Secret1 { get; set; } = string.Empty;
        
        [SensitiveData]
        public string Secret2 { get; set; } = string.Empty;
    }
    
    public class OnlyNonSensitiveDataClass
    {
        public string Public1 { get; set; } = string.Empty;
        public int Public2 { get; set; }
        public bool Public3 { get; set; }
    }
}