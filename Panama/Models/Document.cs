using Panama.Core.Interfaces;

namespace Panama.Core
{
    public class Document : IModel
    {
        public Document() { }
        public Document(string key, object value) 
        {
            Key = key;
            Value = value;
        }
        
        public string Key { get; set; }
        public object Value { get; set; }
    }
}
