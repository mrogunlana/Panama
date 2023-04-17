using Panama.Interfaces;

namespace Panama.Models.Options
{
    public class PanamaOptions : IModel 
    { 
        private Builder _builder;

        public Builder Builder => _builder;

        public PanamaOptions() 
        {
            _builder = new Builder();
        }

        public void SetBuilder(Builder builder) => _builder = builder;
    }
}