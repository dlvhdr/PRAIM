using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRAIM
{
    public static class Utilities
    {
        public static bool Equal(double d1, double d2)
        {
            if (Math.Abs(d1 - d2) <= Double.Epsilon) {
                return true;
            }

            return false;
        }
    }
}
