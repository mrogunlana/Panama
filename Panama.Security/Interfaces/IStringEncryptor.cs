namespace Panama.Security.Interfaces
{
    public interface IStringEncryptor
    {
        string ToString(string value);
        string FromString(string encrypted);
    }
}
