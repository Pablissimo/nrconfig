using Microsoft.Cci;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NRConfigManager.Infrastructure.Cci.Extensions;

namespace NRConfigManager.Infrastructure.Cci
{
    public class CciTypeDetails : CciReferenceDetails, ITypeDetails
    {
        ITypeDefinition _cciType;

        public CciTypeDetails(ITypeDefinition cciType)
            : base(cciType)
        {
            if (cciType == null)
            {
                throw new ArgumentNullException("cciType");
            }

            _cciType = cciType;
        }

        public IAssemblyDetails Assembly
        {
            get { return new CciAssemblyDetails(TypeHelper.GetDefiningUnit(_cciType)); }
        }

        public string Namespace
        {
            get
            {
                IUnitNamespaceReference ns = null;

                if (!this.IsGenericType)
                {
                    ns = TypeHelper.GetDefiningNamespace((INamedTypeDefinition)_cciType);
                }
                else
                {
                    ns = TypeHelper.GetDefiningNamespace((INamedTypeDefinition) ((IGenericTypeInstance)_cciType).GenericType);
                }

                return TypeHelper.GetNamespaceName(ns, NameFormattingOptions.None);
            }
        }
        
        public string Name
        {
            get { return this.FullName.Split('.').Last(); }
        }

        public string FullName
        {
            get 
            {
                if (_cciType is INestedTypeDefinition)
                {
                    return TypeHelper.GetTypeName(((INestedTypeDefinition)_cciType).ContainingTypeDefinition, NameFormattingOptions.OmitTypeArguments | NameFormattingOptions.PreserveSpecialNames) + "+" + TypeHelper.GetTypeName(_cciType, NameFormattingOptions.OmitContainingNamespace | NameFormattingOptions.OmitContainingType);
                }
                else
                {
                    return TypeHelper.GetTypeName(_cciType, NameFormattingOptions.OmitTypeArguments | NameFormattingOptions.PreserveSpecialNames);
                }
            }
        }

        public bool IsGenericParameter
        {
            get { return (_cciType as IGenericParameter) != null; }
        }

        public bool IsGenericTypeDefinition
        {
            get { return _cciType.IsGeneric; }
        }

        public bool IsGenericType
        {
            get { return _cciType is IGenericTypeInstance; }
        }

        public bool IsNested
        {
            get { return _cciType is INestedTypeDefinition; }
        }

        public IEnumerable<ITypeDetails> GenericArguments
        {
            get
            {
                return
                    (_cciType as IGenericTypeInstance)
                    .GenericArguments
                    .Select(x => new CciTypeDetails((ITypeDefinition) x.ResolvedType));
            }
        }

        public bool IsClass
        {
            get { return _cciType.IsClass; }
        }

        public override ITypeDetails DeclaringType
        {
            get 
            {
                return null;
            }
        }

        public IEnumerable<IMethodDetails> GetMethods(System.Reflection.BindingFlags bindingFlags)
        {
            Func<IMethodDefinition, IPropertyDefinition, bool> excludeProperty =
                (method, prop) =>
                {
                    var getter = prop.Getter;
                    var setter = prop.Setter;

                    return getter != null && getter.InternedKey == method.InternedKey
                        || setter != null && setter.InternedKey == method.InternedKey;
                };

            Func<IMethodDefinition, IEventDefinition, bool> excludeEvents =
                (method, @event) =>
                {
                    return @event.Accessors.Any(x => x.InternedKey == method.InternedKey);
                };

            var methods = 
                _cciType
                .Methods
                .Where(method => !method.IsConstructor && method.MatchesFlags(bindingFlags) && !_cciType.Properties.Any(property => excludeProperty(method, property)) && !_cciType.Events.Any(@event => excludeEvents(method, @event)))
                .Select(method => new Cci.CciMethodDetails(method))
                .ToList();

            return methods;
        }

        public IEnumerable<IPropertyDetails> GetProperties(System.Reflection.BindingFlags bindingFlags)
        {
            return
                _cciType
                .Properties
                .Where(property => property.MatchesFlags(bindingFlags))
                .Select(property => new Cci.CciPropertyDetails(property));
        }

        public IEnumerable<IConstructorDetails> GetConstructors(System.Reflection.BindingFlags bindingFlags)
        {
            return
                _cciType
                .Methods
                .Where(method => method.IsConstructor && method.MatchesFlags(bindingFlags))
                .Select(method => new Cci.CciConstructorDetails(method));
        }

        public IEnumerable<ITypeDetails> GetNestedTypes(System.Reflection.BindingFlags bindingFlags)
        {
            return
                _cciType
                .NestedTypes
                .Where(x => x.MatchesFlags(bindingFlags))
                .Select(x => new Cci.CciTypeDetails(x));
        }

        public override string ToString()
        {
            return this.FullName;
        }

        public override bool Equals(object obj)
        {
            CciTypeDetails other = obj as CciTypeDetails;
            if (other != null)
            {
                return other._cciType.InternedKey == _cciType.InternedKey;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return _cciType.InternedKey.GetHashCode();
        }
    }
}
