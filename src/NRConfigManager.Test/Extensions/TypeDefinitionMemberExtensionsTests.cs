using Microsoft.Cci;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NRConfigManager.Infrastructure.Cci.Extensions;

namespace NRConfigManager.Test.Extensions
{
    [TestClass]
    public class TypeDefinitionMemberExtensionsTests
    {
        DummyTypeDefinitionMember _privateStaticMethod;
        DummyTypeDefinitionMember _publicStaticMethod;
        DummyTypeDefinitionMember _privateInstanceMethod;
        DummyTypeDefinitionMember _publicInstanceMethod;

        [TestInitialize]
        public void Initialise()
        {
            _privateStaticMethod = new DummyTypeDefinitionMember("PrivateStaticMethod", false, true);
            _publicStaticMethod = new DummyTypeDefinitionMember("PublicStaticMethod", true, true);
            _privateInstanceMethod = new DummyTypeDefinitionMember("PrivateInstanceMethod", false, false);
            _publicInstanceMethod = new DummyTypeDefinitionMember("PublicInstanceMethod", true, false);
        }

        [TestMethod]
        public void MatchesFlags_ReturnsFalse_IfNeitherPublicNorPrivateFlagsSupplied()
        {
            Assert.IsFalse(_privateStaticMethod.MatchesFlags(System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static));
            Assert.IsFalse(_privateInstanceMethod.MatchesFlags(System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static));
            Assert.IsFalse(_publicStaticMethod.MatchesFlags(System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static));
            Assert.IsFalse(_publicInstanceMethod.MatchesFlags(System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static));
        }

        [TestMethod]
        public void MatchesFlags_ReturnsTrueOnPublicMethodAndFalseOtherwise_IfPublicFlagSpecified()
        {
            Assert.IsFalse(_privateInstanceMethod.MatchesFlags(System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public));
            Assert.IsFalse(_privateStaticMethod.MatchesFlags(System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public));
            Assert.IsTrue(_publicInstanceMethod.MatchesFlags(System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public));
            Assert.IsTrue(_publicStaticMethod.MatchesFlags(System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public));
        }

        [TestMethod]
        public void MatchesFlags_ReturnsTrueOnNonPublicMethodAndFalseOtherwise_IfNonPublicFlagSpecified()
        {
            Assert.IsTrue(_privateInstanceMethod.MatchesFlags(System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic));
            Assert.IsTrue(_privateStaticMethod.MatchesFlags(System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic));
            Assert.IsFalse(_publicInstanceMethod.MatchesFlags(System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic));
            Assert.IsFalse(_publicStaticMethod.MatchesFlags(System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic));
        }

        [TestMethod]
        public void MatchesFlags_ReturnsTrueOnInstanceMethodAndFalseOtherwise_IfInstanceFlagSpecified()
        {
            Assert.IsTrue(_privateInstanceMethod.MatchesFlags(System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic));
            Assert.IsFalse(_privateStaticMethod.MatchesFlags(System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic));
            Assert.IsTrue(_publicInstanceMethod.MatchesFlags(System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic));
            Assert.IsFalse(_publicStaticMethod.MatchesFlags(System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic));
        }

        [TestMethod]
        public void MatchesFlags_ReturnsTrueOnStaticMethodAndFalseOtherwise_IfStaticFlagSpecified()
        {
            Assert.IsFalse(_privateInstanceMethod.MatchesFlags(System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic));
            Assert.IsTrue(_privateStaticMethod.MatchesFlags(System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic));
            Assert.IsFalse(_publicInstanceMethod.MatchesFlags(System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic));
            Assert.IsTrue(_publicStaticMethod.MatchesFlags(System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic));
        }

        class DummyTypeDefinitionMember : ITypeDefinitionMember, ISignature
        {
            public bool Public { get; private set; }
            public bool IsStatic { get; private set; }

            public DummyTypeDefinitionMember(string name, bool @public, bool @static)
            {
                this.Public = @public;
                this.IsStatic = @static;

                this.Name = new DummyName(name);
            }

            public ITypeDefinition ContainingTypeDefinition
            {
                get { return null; }
            }

            public TypeMemberVisibility Visibility
            {
                get { return this.Public ? TypeMemberVisibility.Public : TypeMemberVisibility.Private; }
            }

            public ITypeReference ContainingType
            {
                get { return null; }
            }

            public ITypeDefinitionMember ResolvedTypeDefinitionMember
            {
                get { return this; }
            }

            public IEnumerable<ICustomAttribute> Attributes
            {
                get { return Enumerable.Empty<ICustomAttribute>(); }
            }

            public void Dispatch(IMetadataVisitor visitor)
            {
                
            }

            public void DispatchAsReference(IMetadataVisitor visitor)
            {
                
            }

            public IEnumerable<ILocation> Locations
            {
                get { return Enumerable.Empty<ILocation>(); }
            }

            public IName Name
            {
                get;
                private set;
            }

            public ITypeDefinition Container
            {
                get { return null; }
            }

            public IScope<ITypeDefinitionMember> ContainingScope
            {
                get { return null; }
            }

            public CallingConvention CallingConvention
            {
                get { return CallingConvention.Default; }
            }

            public IEnumerable<IParameterTypeInformation> Parameters
            {
                get { throw new NotImplementedException(); }
            }

            public IEnumerable<ICustomModifier> ReturnValueCustomModifiers
            {
                get { throw new NotImplementedException(); }
            }

            public bool ReturnValueIsByRef
            {
                get { return false; }
            }

            public bool ReturnValueIsModified
            {
                get { return false; }
            }

            public ITypeReference Type
            {
                get { return null; }
            }

            class DummyName : IName
            {
                public DummyName(string name)
                {
                    this.Value = name;
                }

                public int UniqueKey
                {
                    get { return this.Value.GetHashCode(); }
                }

                public int UniqueKeyIgnoringCase
                {
                    get { return this.Value.ToLower().GetHashCode(); }
                }

                public string Value
                {
                    get;
                    private set;
                }
            }
        }
    }
}
