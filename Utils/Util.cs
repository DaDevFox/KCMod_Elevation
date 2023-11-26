using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Elevation.Utils
{
    public static class Util
    {
        public static float RoundToFactor(float a, float factor)
        {
            return Mathf.Round(a / factor) * factor;
        }

        public static float FloorToFactor(float a, float factor)
        {
            return Mathf.Floor(a / factor) * factor;
        }

        public static float CeilToFactor(float a, float factor)
        {
            return Mathf.Ceil(a / factor) * factor;
        }
    }
}
