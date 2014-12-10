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

        public static string PickNewName(List<string> existing, string prefix)
        {
            if (existing == null) return null;

            List<string> prefixes = existing.FindAll(x => x.StartsWith(prefix));
            int max_num = 0;

            foreach (string name in prefixes) {
                if (name.Length < prefix.Length + 1) continue;
                
                string number_str = name.Substring(prefix.Length);
                int number = 0;
                if (int.TryParse(number_str, out number)) {
                    if (number > max_num) {
                        max_num = number;
                    }
                }
            }

            return prefix + (max_num + 1).ToString();
        }
    }
}
