using Panama.Security.Interfaces;

namespace Panama.Security.Resolvers
{
    public enum StringEncryptorResolverKey
    {
        AES,
        SHA256,
        Base64
    }
    public delegate IStringEncryptor StringEncryptorResolver(StringEncryptorResolverKey key);
}
