namespace Panama.Security.Interfaces
{
    public interface ISaltGenerator
    {
        string Salt();
        string Vector();
    }
}
