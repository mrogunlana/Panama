﻿using Microsoft.Extensions.Configuration;
using Panama.Extensions;
using Panama.Interfaces;
using System.Reflection;

namespace Panama.Models
{
    public class Builder : IModel 
    {
        public Options.PanamaOptions? Options { get; }
        public IList<IRegistrar> Registrars { get; }
        public IConfiguration? Configuration { get; }
        public IList<Assembly>? Assemblies { get; }

        public Builder()
        {
            Registrars = new List<IRegistrar>();
        }

        public Builder(
            Options.PanamaOptions options, 
            IConfiguration? configuration = null, 
            IEnumerable<Assembly>? assemblies = null)
            : this()
        {
            Options = options;
            Configuration = configuration;
            Assemblies = new List<Assembly>(assemblies.GetServiceAssemblies());
        }

        public void Register(IRegistrar registrar)
        {
            if (registrar == null)
                throw new ArgumentNullException(nameof(registrar));

            Registrars.Add(registrar);
        }
    }
}