using Panama.Interfaces;

namespace Panama.Models.Options
{
    public class OptionBuilder : IModel 
    { 
        private Builder _builder;

        public Builder Builder => _builder;

        public OptionBuilder() 
        {
            _builder = new Builder();
        }

        public void SetBuilder(Builder builder) => _builder = builder;

        public void Register(IRegistrar registrar)
        {
            if (Builder == null)
                throw new ArgumentNullException(nameof(Builder));

            Builder.Register(registrar);
        }
    }
}