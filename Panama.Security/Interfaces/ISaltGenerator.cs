namespace Panama.Core.Security.Interfaces
{
    public interface ISaltGenerator
    {
        string Salt();
        string Vector();
    }
}
