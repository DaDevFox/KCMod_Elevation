using System.Diagnostics;
using System.Reflection;

namespace Fox.Profiling
{
    internal class MethodCall
    {
        public MethodCall(float elapsedTime, StackTrace stackTrace)
        {
            this.elapsedTime = elapsedTime;
            this.stackTrace = stackTrace;
            origin = stackTrace.GetFrame(1).GetMethod();
        }

        public float elapsedTime = 0f;
        public StackTrace stackTrace;
        public MethodBase origin;
    }
}
