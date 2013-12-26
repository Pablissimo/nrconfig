using NRConfigManager.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRConfigManager.Test.Infrastructure.Support
{
    public class DummyMethodDetails : IMethodDetails
    {
        public DummyMethodDetails(ITypeDetails declaringType, string name)
        {
            this.DeclaringType = declaringType;
            this.Name = name;
        }

        public bool ContainsGenericParameters
        {
            get;
            set;
        }

        public ITypeDetails[] GenericArguments
        {
            get;
            set;
        }

        public IParameterDetails[] Parameters
        {
            get;
            set;
        }

        public string Name
        {
            get;
            private set;
        }

        public ITypeDetails DeclaringType
        {
            get;
            private set;
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
