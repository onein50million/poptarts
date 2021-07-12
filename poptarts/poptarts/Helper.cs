using System;
using System.Collections.Generic;
using System.Text;

namespace poptarts
{
    static class Helper
    {
        public static double Logistic(double input, double max_value = 1.0, double midpoint = 0.0, double steepness=1)
        {
            return max_value / (1 + Math.Exp(-steepness * (input - midpoint)));
        }
    }
}
