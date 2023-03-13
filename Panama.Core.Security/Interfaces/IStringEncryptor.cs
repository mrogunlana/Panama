using Panama.Core.Security.Resolvers;

namespace Panama.Core.Security.Interfaces
{
    public interface IStringEncryptor
    {
        StringEncryptorResolverKey Key { get; }
        string ToString(string value);
        string FromString(string encrypted);
    }
}
