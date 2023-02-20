namespace Panama.Core.Security.Interfaces
{
    public interface IBase10Encryptor
    {
        string ToString(long value);
        long FromString(string encrypted);
    }
}
