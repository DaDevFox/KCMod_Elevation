using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elevation
{
    public static class Submods
    {
        public static Dictionary<string, Action<bool>> submodActivators { get; } = new Dictionary<string, Action<bool>>();






    }
}
