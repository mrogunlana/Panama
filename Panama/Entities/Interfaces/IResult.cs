using System.Collections.Generic;

namespace Panama.Core.Entities
{
    public interface IResult
    {
        List<IModel> Data { get; set; }
        IEnumerable<string> Messages { get; }
        void AddMessage(string message);
        bool Success { get; set; }
        bool Cancelled { get; set; }
    }
}
