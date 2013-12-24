using NRConfig;
using NRConfigManager.Infrastructure.Reflected.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NRConfigManager.Infrastructure.Reflected
{
    public class ReflectedMemberInfoDetails : IInstrumentable
    {
        MemberInfo _memberInfo;

        public NRConfig.InstrumentAttribute InstrumentationContext
        {
            get
            {
                return _memberInfo.GetInstrumentAttribute();
            }
        }

        public bool IsCompilerGenerated
        {
            get { return _memberInfo.GetCustomAttribute(typeof(CompilerGeneratedAttribute)) != null; }
        }

        public ITypeDetails DeclaringType
        {
            get { return new ReflectedTypeDetails(_memberInfo.DeclaringType); }
        }

        public ReflectedMemberInfoDetails(MemberInfo memberInfo)
        {
            _memberInfo = memberInfo;
        }
        
        public override bool Equals(object obj)
        {
            var other = obj as ReflectedMemberInfoDetails;
            if (other != null)
            {
                return _memberInfo.Equals(other._memberInfo);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return _memberInfo.GetHashCode();
        }

        public override string ToString()
        {
            return _memberInfo.ToString();
        }
    }
}
