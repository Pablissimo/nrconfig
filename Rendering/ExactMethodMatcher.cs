using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewRelicConfigManager.Rendering
{
    public class ExactMethodMatcher
    {
        public string MethodName { get; set; }
        public string ParameterTypes { get; set; }

        public ExactMethodMatcher()
        {

        }

        public ExactMethodMatcher(string methodName, string[] parameterTypes)
        {
            this.MethodName = methodName;
            if (parameterTypes != null && parameterTypes.Any())
            {
                this.ParameterTypes = string.Join(",", parameterTypes);
            }
        }
    }
}
