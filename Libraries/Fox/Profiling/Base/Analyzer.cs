using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elevation;

namespace Fox.Profiling
{
    public class Analyzer
    {
        public ProfilerData Data { get; private set; }

        public Analyzer(ProfilerData data)
        {
            this.Data = data;
        }

        public string AverageMethodCallTime()
        {
            string s = "";

            float sum = 0f;

            foreach (MethodCall call in Data.CallData)
                if(call != null)
                    sum += call.elapsedTime;

            float average = sum / (float)Data.CallData.Count;

            s += "---\n";
            s += "Average Method Call Time\n";
            s += average.ToString();

            return s;
        }

        public string AverageMethodCallTimePerMethod()
        {
            string s = "";

            int cutoff = 10;

            #region Data Collection

            // Sort method calls from the same origin into lists
            Dictionary<string, List<MethodCall>> methodCallLookup = new Dictionary<string, List<MethodCall>>();

            foreach (MethodCall call in Data.CallData)
            {
                string m = call.origin.DeclaringType.Name + ":" + call.origin.Name;
                if (!methodCallLookup.ContainsKey(m))
                    methodCallLookup.Add(m, new List<MethodCall>());

                methodCallLookup[m].Add(call);
            }

            // Get the average elapsed time of each of those origins
            Dictionary<string, float> averageTimes = new Dictionary<string, float>();

            foreach (string m in methodCallLookup.Keys) {
                averageTimes.Add(m, 0f);
                foreach (MethodCall c in methodCallLookup[m]) {
                    averageTimes[m] += c.elapsedTime;
                }
                averageTimes[m] /= (float)methodCallLookup[m].Count;
            }

            // Sort by time
            averageTimes.OrderByDescending((pair) => pair.Value);

            #endregion

            s += "---\n";
            s += "Average Method Call Time Per Method\n";

            for(int i = 0; i < Math.Min(cutoff, averageTimes.Keys.Count); i++)
            {
                string method = averageTimes.Keys.ElementAt(i);

                s += method + "\t| " + averageTimes[method] + "\n";
            }

            return s;
        }

        public string LongestMethodCalls()
        {
            string s = "";

            int amt = 10;
            int i = 0;

            s += $"---\nTop {amt} Longest Method Calls\n";

            List<MethodCall> found;

            found = Data.CallData.OrderByDescending(m => m.elapsedTime).Where((m) =>
            {
                i++;
                return i < amt;

            }).ToList();

            found.ForEach((m) =>
            {
                s += $"{m.origin.Name}\t| {m.elapsedTime}\n{m.stackTrace}\n";
            });

            return s;
        }

    }
}
