using System;
using Microsoft.Extensions.Compliance.Classification;
using Microsoft.Extensions.Compliance.Redaction;

namespace Obfuscator.Redactors
{
    public class SimpleRedactorProvider : IRedactorProvider
    {
        public Redactor GetRedactor(DataClassificationSet classifications)
            => new SimpleRedactor();
        
        private sealed class SimpleRedactor : Redactor
        {
            public override int Redact(ReadOnlySpan<char> source, Span<char> destination)
            {
                throw new NotImplementedException();
            }

            public override string Redact(string? source) 
                => source == null ? string.Empty : "[REDACTED]";

            public override int GetRedactedLength(ReadOnlySpan<char> input) 
                => input.Length;
            
        }
        
        
    }
}