using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NRConfigManager.Infrastructure
{
    public interface ITypeDetails : IInstrumentable
    {
        IAssemblyDetails Assembly { get; }

        string Namespace { get; }
        string Name { get; }
        string FullName { get; }
        bool IsGenericParameter { get; }
        bool IsGenericType { get; }
        bool IsGenericTypeDefinition { get; }
        IEnumerable<ITypeDetails> GenericArguments { get; }
        bool IsClass { get; }

        IEnumerable<IMethodDetails> GetMethods(BindingFlags bindingFlags);
        IEnumerable<IPropertyDetails> GetProperties(BindingFlags bindingFlags);
        IEnumerable<IConstructorDetails> GetConstructors(BindingFlags bindingFlags);
    }
}
