using Microsoft.Extensions.Compliance.Redaction;
using Microsoft.Extensions.DependencyInjection;
using Obfuscator.Interfaces;
using Obfuscator.Redactors;

namespace Obfuscator.Extensions
{
    public static class ObfuscatorExtensions
    {
        public static IServiceCollection AddObfuscator(this IServiceCollection services)
        {
            services.AddSingleton<IObfuscatorService, ObfuscatorService>();
            services.AddSingleton<IRedactorProvider, SimpleRedactorProvider>();

            services.AddRedaction(redactionBuilder => { });
            
            return services;
        }
    }
}