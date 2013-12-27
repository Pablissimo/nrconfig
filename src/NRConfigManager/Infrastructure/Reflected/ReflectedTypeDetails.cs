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
            get 
            {
                if (_type.FullName != null)
                {
                    return _type.FullName;
                }
                else if (_type.IsGenericParameter)
                {
                    return _type.Name;
                }
                else
                {
                    return string.Format("{0}.{1}", _type.Namespace, _type.Name); 
                }
            }
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
            Func<MethodInfo, EventInfo, bool> excludeEvents =
                (method, @event) =>
                {
                    return
                        @event.AddMethod != null && @event.AddMethod == method
                        || @event.RemoveMethod != null && @event.RemoveMethod == method;
                };

            Func<MethodInfo, PropertyInfo, bool> excludeProperties =
                (method, property) =>
                {
                    return (property.CanRead && property.GetMethod == method)
                    || (property.CanWrite && property.SetMethod == method);
                };

            var propAndEventFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;

            return
                _type
                .GetMethods(bindingFlags)
                .Where(x => !x.IsConstructor && !x.IsAbstract && !_type.GetProperties(propAndEventFlags).Any(p => excludeProperties(x, p)) && !_type.GetEvents(propAndEventFlags).Any(e => excludeEvents(x, e)))
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
