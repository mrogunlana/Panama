using Panama.Security.Resolvers;

namespace Panama.Security.Interfaces
{
    public interface IStringEncryptor
    {
        StringEncryptorResolverKey Key { get; }
        string ToString(string value);
        string FromString(string encrypted);
    }
}
