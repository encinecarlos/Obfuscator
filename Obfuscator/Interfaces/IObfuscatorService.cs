using System.Threading;
using System.Threading.Tasks;

namespace Obfuscator.Interfaces
{
    public interface IObfuscatorService
    {
        string SanitizeSensitiveData<T>(T input);
    }
}