using Autofac;
using Autofac.Features.AttributeFilters;
using Panama.Commands;
using System;
using System.Linq;

namespace Panama.IoC.Autofac
{
    public class DefaultAutofacConfiguration
    {
        public ContainerBuilder Register(ContainerBuilder builder)
        {
            //Register all validators -- singletons
            builder.RegisterAssemblyTypes(AppDomain.CurrentDomain.GetAssemblies())
                   .Where(t => t.IsAssignableTo<IValidation>())
                   .Named<IValidation>(t => t.Name)
                   .AsImplementedInterfaces()
                   .SingleInstance();

            //Register all commands -- singletons
            builder.RegisterAssemblyTypes(AppDomain.CurrentDomain.GetAssemblies())
                   .Where(t => t.IsAssignableTo<ICommand>())
                   .Named<ICommand>(t => t.Name)
                   .AsImplementedInterfaces()
                   .SingleInstance()
                   .WithAttributeFiltering();

            return builder;
        }
    }
}
