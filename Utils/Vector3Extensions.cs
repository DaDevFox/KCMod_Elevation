using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Elevation.Utils
{
    public static class Vector3Extensions
    {
        public static void SetX(this Vector3 vector, float x)
        {
            vector.x = x;
        }
    }
}
