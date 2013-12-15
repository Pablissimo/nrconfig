using Microsoft.Cci;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NRConfigManager.Infrastructure.Cci
{
    public class CciTypeDetails : CciReferenceDetails, ITypeDetails
    {
        INamedTypeDefinition _cciType;

        public CciTypeDetails(INamedTypeDefinition cciType)
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

        public string Name
        {
            get { return this.FullName.Split('.').Last(); }
        }

        public string FullName
        {
            get { return _cciType.Name.Value; }
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
            get { return _cciType.IsGeneric; }
        }

        public IEnumerable<ITypeDetails> GenericArguments
        {
            get
            {
                return
                    (_cciType as IGenericTypeInstance)
                    .GenericArguments
                    .Select(x => new CciTypeDetails((INamedTypeDefinition) x.ResolvedType));
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

            return 
                _cciType
                .Methods
                .Where(method => !method.IsConstructor && MatchesFlags(method, bindingFlags) && !_cciType.Properties.Any(property => excludeProperty(method, property)))
                .Select(method => new Cci.CciMethodDetails(method));
        }

        public IEnumerable<IPropertyDetails> GetProperties(System.Reflection.BindingFlags bindingFlags)
        {
            return
                _cciType
                .Properties
                .Where(property => MatchesFlags(property, bindingFlags))
                .Select(property => new Cci.CciPropertyDetails(property));
        }

        public IEnumerable<IConstructorDetails> GetConstructors(System.Reflection.BindingFlags bindingFlags)
        {
            return
                _cciType
                .Methods
                .Where(method => method.IsConstructor && MatchesFlags(method, bindingFlags))
                .Select(method => new Cci.CciConstructorDetails(method));
        }

        private bool MatchesFlags(ITypeDefinitionMember defn, BindingFlags bindingFlags)
        {
            var defnSignature = defn as ISignature;

            bool isPublic = (defn.Visibility & TypeMemberVisibility.Public) == TypeMemberVisibility.Public;
            bool isStatic = (defnSignature != null ? defnSignature.IsStatic : false);

            bool mayBeNonPublic = (bindingFlags & BindingFlags.NonPublic) == BindingFlags.NonPublic;
            bool mayBePublic = (bindingFlags & BindingFlags.Public) == BindingFlags.Public;
            bool mayBeInstance = (bindingFlags & BindingFlags.Instance) == BindingFlags.Instance;
            bool mayBeStatic = (bindingFlags & BindingFlags.Static) == BindingFlags.Static;

            return (mayBePublic && isPublic) || (mayBeNonPublic && !isPublic) || (mayBeInstance && !isStatic) || (mayBeStatic && isStatic);
        }
    }
}
