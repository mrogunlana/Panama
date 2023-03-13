using System.Collections.Generic;

namespace Panama.Interfaces
{
    public interface IResult
    {
        IList<IModel> Data { get; set; }
        IList<string> Messages { get; }
        void AddMessage(string message);
        bool Success { get; set; }
        bool Cancelled { get; set; }
    }
}
