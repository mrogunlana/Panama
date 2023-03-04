using Panama.Core.Security.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Panama.Core.Security.Resolvers
{
    public enum ResolverKey
    {
        AES,
        SHA256,
        Base64
    }
    public delegate IStringEncryptor StringEncryptorResolver(ResolverKey key);
}
