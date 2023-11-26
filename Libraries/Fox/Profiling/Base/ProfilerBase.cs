using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fox.Profiling
{
    public class ProfilerBase
    {
        protected static KCModHelper _helper;

        public ProfilerData Data { get; protected set; } = new ProfilerData();
        public bool Active { get; private set; } = true;

        void Preload(KCModHelper helper)
        {
            _helper = helper;
        }

        #region Activation

        // TODO: Finish Profiler
        public void Init()
        {
            this.Activate();
            this.Inject();
        }

        public void Activate()
        {
            Active = true;
        }

        public void Deactivate()
        {
            Active = false;
            PrintSummary();
        }

        #endregion

        #region Analysis

        public void print(object message)
        {
            _helper.Log(message.ToString());
        }

        protected void PrintSummary()
        {
            Analyzer analyzer = new Analyzer(Data);

            print("-----------------------------");
            print("----- Profiling Summary -----");
            print("-----------------------------");

            // Average method call time for all
            print(analyzer.AverageMethodCallTime());

            // Average method call time per method
            print(analyzer.AverageMethodCallTimePerMethod());

            // Longest individual method calls and params
            print(analyzer.LongestMethodCalls());
        }

        #endregion

        #region Injection

        protected virtual void Inject()
        {

        }

        protected virtual void Uninject()
        {

        }

        #endregion

    }
}
