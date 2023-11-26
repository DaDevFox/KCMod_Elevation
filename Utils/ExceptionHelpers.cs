using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elevation
{
    public static class ExceptionHelpers
    {
        public static void CheckNull(this Object obj, string name = "obj")
        {
            if (obj == null)
                throw new ArgumentNullException(name);
        }
    }
}
