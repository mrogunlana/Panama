using Panama.Core.Entities;
using System.Collections.Generic;
using System.Threading;

namespace Panama.Core.Commands
{
    public class Subject
    {
        public Subject()
        {
            if (Data == null)
                Data = new List<IModel>();
        }
        public Subject(List<IModel> data, CancellationToken token) 
            : this()
        {
            Data = data;
            Token = token;
        }
        public List<IModel> Data { get; set; }
        public CancellationToken Token { get; set; }
    }
}
