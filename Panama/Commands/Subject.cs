using Panama.Core.Entities;
using System.Collections.Generic;
using System.Threading;

namespace Panama.Core.Commands
{
    public class Subject
    {
        public Subject()
        {
            if (Context == null)
                Context = new List<IModel>();
        }
        public Subject(List<IModel> context, CancellationToken token) 
            : this()
        {
            Context = context;
            Token = token;
        }
        public List<IModel> Context { get; set; }
        public CancellationToken Token { get; set; }
    }
}
