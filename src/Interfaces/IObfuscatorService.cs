namespace Obfuscator.Interfaces
{
    public interface IObfuscatorService
    {
        string ObfuscateSensitiveData<T>(T input);
    }
}