using NRConfigManager.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRConfigManager.Test.Infrastructure.Support
{
    public class DummyAssembly : IAssemblyDetails
    {
        static DummyAssembly _instance = GetDefaultInstance();

        public static DummyAssembly Instance { get { return _instance; } }

        public DummyAssembly(string assyFullname, NRConfig.InstrumentAttribute assyLevelAttribute)
            : this(assyFullname)
        {
            this.InstrumentationContext = assyLevelAttribute;
        }

        public DummyAssembly(string assyFullname)
        {
            this.FullName = assyFullname;
            this.Name = assyFullname.Split('.').Last();
        }

        public string FullName
        {
            get;
            private set;
        }

        public string Name
        {
            get;
            private set;
        }

        public ITypeDetails DeclaringType
        {
            get { return null; }
        }

        public NRConfig.InstrumentAttribute InstrumentationContext
        {
            get;
            set;
        }

        public bool IsCompilerGenerated
        {
            get { return false; }
        }

        public static void Reset()
        {
            _instance = GetDefaultInstance();
        }

        private static DummyAssembly GetDefaultInstance()
        {
            return new DummyAssembly("TestAssembly.Subname");
        }
    }
}
