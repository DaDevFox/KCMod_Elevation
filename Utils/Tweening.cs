// credit to ChatGPT 3.5 for this quick replacement of DG.Tweening
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Elevation.Utils
{

    public class TweeningManager : MonoBehaviour
    {
        private static TweeningManager instance;
        public static TweeningManager Instance => instance;
        private List<Tweener> tweeners = new List<Tweener>();
        private List<Tweener> deferredRemoval = new List<Tweener>();

        void Start()
        {
            //if (instance != null && instance != this)
            //{
            //    Destroy(gameObject);
            //}
            //else
            //{
            instance = this;
            DontDestroyOnLoad(gameObject);
            //}
        }

        // Tween a value over time, continuously acting on the current value using a lambda function
        public void TweenValue(float startValue, Func<float, float> updateAction, float duration)
        {
            // Create a new tweener and start tweening
            Tweener newTweener = new Tweener(startValue, updateAction, duration);
            newTweener.StartTween();

            tweeners.Add(newTweener);
        }

        public void Update()
        {
            for (int i = 0; i < tweeners.Count; i++)
            {
                Tweener tweener = tweeners[i];
                tweener.Update();
                if (!tweener.isTweening)
                    deferredRemoval.Add(tweener);
            }

            if (deferredRemoval.Count > 0)
            {
                for (int i = 0; i < deferredRemoval.Count; i++)
                    deferredRemoval.Remove(deferredRemoval[i]);
                deferredRemoval.Clear();
            }
        }
    }

    public class Tweener
    {
        private float startValue;
        private Func<float, float> updateAction;
        private float duration;
        private float elapsed;

        public bool isTweening;
        private float startTime;
        private float currentValue;
        private float endValue;

        public Tweener(float startValue, Func<float, float> updateAction, float duration)
        {
            this.startValue = startValue;
            this.updateAction = updateAction;
            this.duration = duration;
        }

        public void StartTween()
        {
            isTweening = true;
            startTime = Time.time;
            currentValue = startValue;
            endValue = updateAction(startValue);
        }

        public void StopTween()
        {
            isTweening = false;
        }

        public void Update()
        {
            if (isTweening)
            {
                elapsed += Time.unscaledDeltaTime;

                float t = Mathf.Clamp01(elapsed / duration);
                currentValue = Mathf.Lerp(startValue, endValue, t);
                updateAction?.Invoke(currentValue);
                Mod.dLog("tweening");

                if (t >= 1f || Mathf.Approximately(currentValue, endValue))
                {
                    updateAction?.Invoke(endValue);
                    isTweening = false;
                }
            }
        }
    }
}