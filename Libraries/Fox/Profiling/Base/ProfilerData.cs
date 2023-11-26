using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fox.Profiling
{
    public class ProfilerData
    {
        internal List<MethodCall> CallData { get; private set; } = new List<MethodCall>();


        internal void Add(MethodCall call)
        {
            CallData.Add(call);
        }
    }
}
