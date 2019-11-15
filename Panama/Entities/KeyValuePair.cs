namespace Panama.Core.Entities
{
    public class KeyValuePair : IModel
    {
        public KeyValuePair() { }
        public KeyValuePair(string key, object value) 
        {
            Key = key;
            Value = value;
        }
        
        public string Key { get; set; }
        public object Value { get; set; }
    }
}
