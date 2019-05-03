using System.Collections.Generic;

namespace Panama.Entities
{
    public interface IResult
    {
        List<IModel> Data { get; set; }
        IEnumerable<string> Messages { get; }
        void AddMessage(string message);
        bool Success { get; set; }
    }
}
