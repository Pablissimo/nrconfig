using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace NRConfigManager.Infrastructure.Reflected
{
    public class ReflectedTypeDetails : ReflectedMemberInfoDetails, ITypeDetails
    {
        Type _type;

        public IAssemblyDetails Assembly
        {
            get { return new ReflectedAssemblyDetails(_type.Assembly); }
        }

        public string Namespace
        {
            get { return _type.Namespace; }
        }

        public string Name
        {
            get { return _type.Name; }
        }

        public string FullName
        {
            get { return _type.FullName; }
        }

        public bool IsGenericParameter
        {
            get { return _type.IsGenericParameter; }
        }

        public bool IsGenericTypeDefinition
        {
            get { return _type.IsGenericTypeDefinition; }
        }

        public bool IsGenericType
        {
            get { return _type.IsGenericType; }
        }

        public bool IsNested
        {
            get { return _type.IsNested; }
        }

        public IEnumerable<ITypeDetails> GenericArguments
        {
            get 
            { 
                return 
                    _type
                    .GetGenericArguments()
                    .Select(x => new ReflectedTypeDetails(x)); 
            }
        }

        public bool IsClass
        {
            get { return _type.IsClass; }
        }

        public IEnumerable<IMethodDetails> GetMethods(System.Reflection.BindingFlags bindingFlags)
        {
            return
                _type
                .GetMethods(bindingFlags)
                .Select(x => new ReflectedMethodDetails(x));
        }

        public IEnumerable<IPropertyDetails> GetProperties(System.Reflection.BindingFlags bindingFlags)
        {
            return
                _type
                .GetProperties(bindingFlags)
                .Select(x => new ReflectedPropertyDetails(x));
        }

        public IEnumerable<IConstructorDetails> GetConstructors(System.Reflection.BindingFlags bindingFlags)
        {
            return
                _type
                .GetConstructors(bindingFlags)
                .Select(x => new ReflectedConstructorDetails(x));
        }

        public IEnumerable<ITypeDetails> GetNestedTypes(System.Reflection.BindingFlags bindingFlags)
        {
            return
                _type
                .GetNestedTypes(bindingFlags)
                .Select(x => new ReflectedTypeDetails(x));
        }

        public ReflectedTypeDetails(Type type)
            : base(type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            _type = type;
        }
    }
}
