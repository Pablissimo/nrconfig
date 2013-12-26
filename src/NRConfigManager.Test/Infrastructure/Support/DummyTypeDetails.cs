using NRConfigManager.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NRConfigManager.Test.Infrastructure.Support
{
    public class DummyTypeDetails : ITypeDetails
    {
        IEnumerable<IMethodDetails> _methods;
        IEnumerable<IConstructorDetails> _constructors;
        IEnumerable<IPropertyDetails> _properties;
        IEnumerable<ITypeDetails> _nestedTypes;

        BindingFlags _lastGetMethodsFlags;
        BindingFlags _lastGetConstructorsFlags;
        BindingFlags _lastGetPropertiesFlags;
        BindingFlags _lastGetNestedTypesFlags;

        public BindingFlags LastGetMethodsFlags { get { return _lastGetMethodsFlags; } }
        public BindingFlags LastGetConstructorsFlags { get { return _lastGetConstructorsFlags; } }
        public BindingFlags LastGetPropertiesFlags { get { return _lastGetPropertiesFlags; } }
        public BindingFlags LastGetNestedTypesFlags { get { return _lastGetNestedTypesFlags; } }

        public DummyTypeDetails(string fullname)
        {
            this.FullName = fullname;
            this.IsClass = true;
        }

        public IAssemblyDetails Assembly
        {
            get { return DummyAssembly.Instance; }
        }

        public string Namespace
        {
            get 
            {
                return this.FullName.Substring(0, this.FullName.LastIndexOf('.'));
            }
        }

        public string Name
        {
            get { return this.FullName.Split('.').Last(); }
        }

        public string FullName
        {
            get;
            set;
        }

        public bool IsGenericParameter
        {
            get;
            set;
        }

        public bool IsGenericType
        {
            get;
            set;
        }

        public bool IsGenericTypeDefinition
        {
            get;
            set;
        }

        public IEnumerable<ITypeDetails> GenericArguments
        {
            get;
            set;
        }

        public bool IsClass
        {
            get;
            set;
        }

        public bool IsNested
        {
            get;
            set;
        }

        public void SetMethods(IEnumerable<IMethodDetails> methods)
        {
            _methods = methods;
        }

        public IEnumerable<IMethodDetails> GetMethods(System.Reflection.BindingFlags bindingFlags)
        {
            _lastGetMethodsFlags = bindingFlags;
            return _methods ?? Enumerable.Empty<IMethodDetails>();
        }

        public void SetProperties(IEnumerable<IPropertyDetails> properties)
        {
            _properties = properties;
        }

        public IEnumerable<IPropertyDetails> GetProperties(System.Reflection.BindingFlags bindingFlags)
        {
            _lastGetPropertiesFlags = bindingFlags;
            return _properties ?? Enumerable.Empty<IPropertyDetails>();
        }

        public void SetConstructors(IEnumerable<IConstructorDetails> constructors)
        {
            _constructors = constructors;
        }

        public IEnumerable<IConstructorDetails> GetConstructors(System.Reflection.BindingFlags bindingFlags)
        {
            _lastGetConstructorsFlags = bindingFlags;
            return _constructors ?? Enumerable.Empty<IConstructorDetails>();
        }

        public void SetNestedTypes(IEnumerable<ITypeDetails> nestedTypes)
        {
            _nestedTypes = nestedTypes;
        }

        public IEnumerable<ITypeDetails> GetNestedTypes(System.Reflection.BindingFlags bindingFlags)
        {
            _lastGetNestedTypesFlags = bindingFlags;
            return _nestedTypes ?? Enumerable.Empty<ITypeDetails>();
        }

        public ITypeDetails DeclaringType
        {
            get;
            set;
        }

        public NRConfig.InstrumentAttribute InstrumentationContext
        {
            get;
            set;
        }

        public bool IsCompilerGenerated
        {
            get;
            set;
        }
    }
}
