using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Diagnostics;
using Harmony;
using Elevation;
using UnityEngine;

namespace Fox.Profiling
{
    public class SelectiveProfiler : ProfilerBase
    {
        public static readonly SelectiveProfiler instance = new SelectiveProfiler();
        private static HarmonyInstance _profilingInstance;

        private static PerformanceCounter _avgCounter64Sample;
        private static PerformanceCounter _avgCounter64SampleBase;

        private List<MethodInfo> _targets = new List<MethodInfo>();
        private List<MethodInfo> _patchedMethods = new List<MethodInfo>();


        private SelectiveProfiler()
        {
            // No external constructor
        }

        /// <summary>
        /// Adds all methods in the given namespace to be profiled. 
        /// </summary>
        /// <param name="_namespace"></param>
        public void AddProfileTarget(string _namespace)
        {
            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
                if (type.Namespace == _namespace)
                    AddProfileTarget(type);
        }

        /// <summary>
        /// Adds all non-inherited methods in a Type to be profiled. 
        /// </summary>
        /// <param name="type"></param>
        public void AddProfileTarget(Type type)
        {
            foreach (MethodInfo method in type.GetMethods())
                if(method.DeclaringType == type)
                    AddProfileTarget(method);
        }


        /// <summary>
        /// Adds the given method to be profiled. 
        /// </summary>
        /// <param name="method"></param>
        public void AddProfileTarget(MethodInfo method)
        {
            if (method == null)
                return;

            Mod.dLog("adding method " + method.Name);
            this._targets.Add(method);
        }

        #region Injection

        protected override void Inject()
        {
            Mod.dLog("Injecting Profiler");


            try
            {
                SetupHarmony();
                RegisterAttributes();
                InjectTypes();
            }
            catch(Exception ex)
            {
                Mod.dLog(ex);
            }
        }

        private void SetupHarmony()
        {
            _profilingInstance = HarmonyInstance.Create("profiler_selective");
        }


        private void RegisterAttributes()
        {
            Type[] allTypes = Assembly.GetExecutingAssembly().GetTypes();
            allTypes.Do( (type) =>
            {
                foreach (MethodInfo method in type.GetMethods())
                    if (method.GetCustomAttribute(typeof(ProfileAttribute)) != null)
                        AddProfileTarget(method);
            });
        }

        private void InjectTypes()
        {
            _patchedMethods.Clear();

            HarmonyMethod prefixPatch = new HarmonyMethod(typeof(SelectiveProfiler), "Prefix");
            HarmonyMethod postfixPatch = new HarmonyMethod(typeof(SelectiveProfiler), "Postfix");

            foreach (MethodInfo info in _targets)
            {
                if (info.DeclaringType == this.GetType())
                    continue;

                MethodBase mBase = info as MethodBase;

                Mod.dLog(info.DeclaringType.Name + "::" + info.Name);
                try
                {
                    _profilingInstance.Patch(mBase, prefixPatch, postfixPatch);
                    _patchedMethods.Add(mBase);
                }
                catch (Exception ex)
                {
                    Mod.dLog(ex);
                }
            }
            Mod.dLog("completed injection");
        }

        #endregion

        #region Uninjection

        protected override void Uninject() 
        {
        }

        #endregion

        private static void TrackMethodCall(MethodCall call)
        {
            instance.Data.Add(call);
        }


        #region Tracker Patches

        private static float currentTrack = -1f;

        static void Prefix()
        {
            if(Mathf.Approximately(currentTrack, -1f))
                currentTrack = Time.time;
        }

        static void Postfix()
        {
            float elapsed = Time.time - currentTrack;
            StackTrace trace = new StackTrace();

            

            MethodCall method = new MethodCall(elapsed, trace);
            TrackMethodCall(method);
            currentTrack = -1f;
        }

        #endregion

    }
}
