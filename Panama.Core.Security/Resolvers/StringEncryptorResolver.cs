using Panama.Core.Security.Interfaces;

namespace Panama.Core.Security.Resolvers
{
    public enum StringEncryptorResolverKey
    {
        AES,
        SHA256,
        Base64
    }
    public delegate IStringEncryptor StringEncryptorResolver(StringEncryptorResolverKey key);
}
