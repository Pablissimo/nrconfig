using Microsoft.Cci;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NRConfigManager.Infrastructure.Cci.Extensions
{
    public static class TypeDefinitionMemberExtensions
    {
        public static bool MatchesFlags(this ITypeDefinitionMember defn, BindingFlags bindingFlags)
        {
            var defnSignature = defn as ISignature;

            bool isPublic = (defn.Visibility & TypeMemberVisibility.Public) == TypeMemberVisibility.Public;
            bool isStatic = (defnSignature != null ? defnSignature.IsStatic : false);

            bool mayBeNonPublic = (bindingFlags & BindingFlags.NonPublic) == BindingFlags.NonPublic;
            bool mayBePublic = (bindingFlags & BindingFlags.Public) == BindingFlags.Public;
            bool mayBeInstance = (bindingFlags & BindingFlags.Instance) == BindingFlags.Instance;
            bool mayBeStatic = (bindingFlags & BindingFlags.Static) == BindingFlags.Static;

            return ((mayBePublic && isPublic) || (mayBeNonPublic && !isPublic)) && ((mayBeInstance && !isStatic) || (mayBeStatic && isStatic));
        }
    }
}